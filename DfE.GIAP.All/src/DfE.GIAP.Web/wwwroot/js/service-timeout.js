$(document).ready(function () {
    var clientActive = false;
    var inActiveSessionExpirationCountdown = false;
    var sessionExpirationModalCountdown = 180;                          // Countdown value used by the popup modal (3 minutes).
    var sessionSignOutRoute = "../auth/signout";
    var keepSessionAliveRoute = "/ServiceTimeout/KeepSessionAlive";
    var SessionTimeoutValueRoute = "/ServiceTimeout/SessionTimeoutValue";
    var currentExpirationCountdown;
    var clientActivityCheckPulse;
    var sessionExpirationCountdown;
    var sessionExpirationCountDownThreshold = setSessionTimeoutValue(); // Time taken to invoke the countdown modal in seconds seconds.
    var sessionCountdown = 0;
    var hasInitialActiveCountdownTimestamp = false;
    var initialActiveCountdownTimestamp;

    hideSessionTimeoutModal();
    checkSessionExpirationOnUserInput();
    checkClientActivity();
    checkCurrentSessionExpirationStatus();

    $('#btnContinue').click(function () {
        hideSessionTimeoutModal();
        sendSessionKeepAlive('keepSessionAlive');
        hasInitialActiveCountdownTimestamp = false;
        inActiveSessionExpirationCountdown = false;
    });

    $('#btnExitService').click(function () {
        hideSessionTimeoutModal();
        location = sessionSignOutRoute;
    });

    function checkCurrentSessionExpirationStatus() {
        checkTabFocused();

        currentExpirationCountdown = setTimeout(function () {
            if (!clientActive) {
                initialActiveCountdownTimestamp = Math.floor(new Date().getTime() / 1000);
                inActiveSessionExpirationCountdown = true;
                displaySessionExpirationCountdown();
                showSessionTimeoutModal();
                setTimeout(function () {
                }, sessionExpirationModalCountdown * 1000);
            }
            else {
                sendSessionKeepAlive('keepSessionAlive');
            }
        }, sessionExpirationCountDownThreshold * 1000);
    }

    function checkClientActivity() {
        clientActivityCheckPulse = setTimeout(function () {
            if (clientActive) {
                sendSessionKeepAlive('keepSessionAlive');
            }
            else {
                clearTimeout(clientActivityCheckPulse);
                checkClientActivity()
            }
        }, 1000);
    }

    function sendSessionKeepAlive(request) {
        $.ajax({
            type: "POST",
            url: keepSessionAliveRoute,
            dataType: "json",
            done: function (response) {
            },
            fail: function (error) {
                console.log("Error posting to " & keepSessionAliveRoute);
            }
        });
        clearTimeout(clientActivityCheckPulse);
        clearTimeout(currentExpirationCountdown);
        clearTimeout(sessionExpirationCountdown);
        clientActive = false;
        checkCurrentSessionExpirationStatus();
        checkClientActivity();
        checkSessionExpirationOnUserInput();
    }

    function checkSessionExpirationOnUserInput() {
        $('body').on('mousemove keydown', function () {
            if (!inActiveSessionExpirationCountdown) {
                clientActive = true;
            }
        });
    }

    function checkTabFocused() {
        if (document.visibilityState === 'hidden' && inActiveSessionExpirationCountdown) {
            hasInitialActiveCountdownTimestamp = true;
        }
    }

    document.addEventListener('visibilitychange', checkTabFocused);

    function displaySessionExpirationCountdown() {
        sessionCountdown = sessionExpirationModalCountdown;
        sessionExpirationCountdown = setInterval(function () {
            sessionCountdown -= 1;
            setCountdownTimerDisplay();
        }, 1000);
    }

    function getCountDownDisplay(counter) {
        var minutes;
        var countdownDisplay;
        if (counter < 60) {
            countdownDisplay = "".concat(counter, " ").concat(counter !== 1 ? 'seconds' : 'second');
        } else {
            minutes = Math.ceil(counter / 60);
            countdownDisplay = "".concat(minutes, " ").concat(minutes === 1 ? 'minute' : 'minutes');
        }
        return countdownDisplay;
    }

    function setSessionTimeoutValue() {
        var sessionTimeoutValue = JSON.parse(getSessionTimeoutValue());
        return (sessionTimeoutValue * 60) - sessionExpirationModalCountdown;
    }

    function getSessionTimeoutValue() {
        return $.ajax({
            async: false,
            type: "GET",
            contentType: "application/json",
            dataType: "text/plain",
            url: SessionTimeoutValueRoute,
            done: function () {
            },
            fail: function (data) {
                console.log("Error posting to " & SessionTimeoutValueRoute);
            }
        })
        .responseText;
    }

    function setCountdownTimerDisplay() {
        var currentCountdownValue;
        if (hasInitialActiveCountdownTimestamp && document.visibilityState === 'visible') {
            var currentTimestamp = Math.floor(new Date().getTime() / 1000);
            var currentTimestampOffset = currentTimestamp - initialActiveCountdownTimestamp;
            hasInitialActiveCountdownTimestamp = false;
            var revisedSessionStorageCountdown = sessionExpirationModalCountdown - currentTimestampOffset;
            if (revisedSessionStorageCountdown <= 0) {
                revisedSessionStorageCountdown = 0;
            }
            sessionCountdown = revisedSessionStorageCountdown;
        }
        currentCountdownValue = sessionCountdown;
        $('#seconds-timer').html(getCountDownDisplay(currentCountdownValue));
        if (currentCountdownValue <= 0) {
            location = sessionSignOutRoute;
            hideSessionTimeoutModal();
        }
    }

    function hideSessionTimeoutModal() {
        $('#seconds-timer').html(null);
        $('#session-expire-warning-modal-overlay').hide();
        $('#session-expire-warning-modal').hide();
    }

    function showSessionTimeoutModal() {
        setCountdownTimerDisplay();
        $('#session-expire-warning-modal-overlay').removeAttr('hidden');
        $('#session-expire-warning-modal-overlay').show();
        $('#session-expire-warning-modal').show();
    }
})