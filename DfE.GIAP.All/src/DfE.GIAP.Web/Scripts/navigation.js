let menuButton = document.getElementsByClassName('govuk-js-header-toggle')[0];
let menu = menuButton && document.getElementById(menuButton.getAttribute('aria-controls'));

/**
 * Initialise header
 *
 * Check for the presence of the header, menu and menu button – if any are
 * missing then there's nothing to do so return early.
 */
let init = function () {
    if (!menuButton || !menu) {
        return
    }

    syncState(menu.classList.contains('govuk-header__navigation--open'))
    menuButton.addEventListener('click', handleMenuButtonClick)
}

/**
 * Sync menu state
 *
 * Sync the menu button class and the accessible state of the menu and the menu
 * button with the visible state of the menu
 *
 * @param {boolean} isVisible Whether the menu is currently visible
 */
let syncState = function (isVisible) {
    menuButton.classList.toggle('govuk-header__menu-button--open', isVisible)
    menuButton.setAttribute('aria-expanded', isVisible)
}

/**
 * Handle menu button click
 *
 * When the menu button is clicked, change the visibility of the menu and then
 * sync the accessibility state and menu button state
 */
let handleMenuButtonClick = function () {
    var isVisible = menu.classList.toggle('govuk-header__navigation--open')
    syncState(isVisible)
}

init();