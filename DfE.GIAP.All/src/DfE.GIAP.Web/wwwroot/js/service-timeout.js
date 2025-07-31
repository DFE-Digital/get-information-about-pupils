$(document).ready(function () {
    const SESSION_MODAL_COUNTDOWN = 180; // 3 minutes
    const KEEP_ALIVE_INTERVAL = 30000;   // 30 seconds
    const SESSION_SIGNOUT_ROUTE = "../auth/signout";
    const KEEP_SESSION_ALIVE_ROUTE = "/ServiceTimeout/KeepSessionAlive";
    const SESSION_TIMEOUT_VALUE_ROUTE = "/ServiceTimeout/SessionTimeoutValue";

    let sessionCountdown = SESSION_MODAL_COUNTDOWN;
    let sessionExpirationThreshold;
    let keepAliveTimer;
    let countdownTimer;
    let modalTimer;
    let clientActive = false;
    let modalVisible = false;
    let lastKeepAliveTime = 0;

    init();

    function init() {
        hideSessionTimeoutModal();
        setupUserActivityTracking();
        fetchSessionTimeoutValue()
            .then(timeout => {
                sessionExpirationThreshold = (timeout * 60) - SESSION_MODAL_COUNTDOWN;
                startSessionMonitor();
            });
    }

    function setupUserActivityTracking() {
        let activityDebounce;

        const markActive = () => {
            clearTimeout(activityDebounce);
            activityDebounce = setTimeout(() => {
                clientActive = true;
            }, 500);
        };

        // Detect mouse, keyboard, scroll, and touch activity
        $('body').on('mousemove keydown scroll touchstart touchmove', markActive);

        // Handle tab visibility and window focus
        const handleVisibilityOrFocus = () => {
            if (document.visibilityState === 'visible' || document.hasFocus()) {
                clientActive = true;

                if (modalVisible) {
                    adjustCountdownForHiddenTab();
                }
            }
        };

        document.addEventListener('visibilitychange', handleVisibilityOrFocus);
        $(window).on('focus', handleVisibilityOrFocus);
    }



    function fetchSessionTimeoutValue() {
        return $.ajax({
            type: "GET",
            url: SESSION_TIMEOUT_VALUE_ROUTE,
            dataType: "json"
        })
            .then(value => value || 20)
            .catch(() => 20);
    }

    function startSessionMonitor() {
        setTimeout(() => {
            if (!clientActive) {
                showSessionTimeoutModal();
                startCountdownTimer();
            } else {
                sendKeepAlive();
            }
        }, sessionExpirationThreshold * 1000);

        keepAliveTimer = setInterval(() => {
            if (clientActive) {
                sendKeepAlive();
                clientActive = false;
            }
        }, KEEP_ALIVE_INTERVAL);
    }

    function sendKeepAlive() {
        const now = Date.now();
        if (now - lastKeepAliveTime < KEEP_ALIVE_INTERVAL) return;
        lastKeepAliveTime = now;

        $.ajax({
            type: "POST",
            url: KEEP_SESSION_ALIVE_ROUTE,
            dataType: "json"
        }).fail(() => {
            console.warn("Keep-alive request failed");
        });
    }

    function showSessionTimeoutModal() {
        modalVisible = true;
        $('#session-expire-warning-modal-overlay').show();
        $('#session-expire-warning-modal').show();
        updateCountdownDisplay();
    }

    function hideSessionTimeoutModal() {
        modalVisible = false;
        $('#session-expire-warning-modal-overlay').hide();
        $('#session-expire-warning-modal').hide();
        $('#seconds-timer').html('');
        clearInterval(countdownTimer);
    }

    function startCountdownTimer() {
        countdownTimer = setInterval(() => {
            sessionCountdown--;
            updateCountdownDisplay();
            if (sessionCountdown <= 0) {
                location = SESSION_SIGNOUT_ROUTE;
                hideSessionTimeoutModal();
            }
        }, 1000);
    }

    function adjustCountdownForHiddenTab() {
        const now = Math.floor(Date.now() / 1000);
        const offset = now - (SESSION_MODAL_COUNTDOWN - sessionCountdown);
        sessionCountdown = Math.max(0, sessionCountdown - offset);
    }

    function updateCountdownDisplay() {
        const display = sessionCountdown < 60
            ? `${sessionCountdown} ${sessionCountdown !== 1 ? 'seconds' : 'second'}`
            : `${Math.ceil(sessionCountdown / 60)} ${Math.ceil(sessionCountdown / 60) === 1 ? 'minute' : 'minutes'}`;
        $('#seconds-timer').html(display);
    }

    $('#btnContinue').click(() => {
        hideSessionTimeoutModal();
        sendKeepAlive();
        sessionCountdown = SESSION_MODAL_COUNTDOWN;
    });

    $('#btnExitService').click(() => {
        hideSessionTimeoutModal();
        location = SESSION_SIGNOUT_ROUTE;
    });
});
