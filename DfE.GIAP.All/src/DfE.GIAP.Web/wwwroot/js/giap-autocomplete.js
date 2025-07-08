var selectEl = document.querySelector('#security-reports--organisation--dropdown')
if (selectEl === null)
    var selectEl = document.querySelector('#security-reports--establishment--dropdown')
accessibleAutocomplete.enhanceSelectElement({
    minLength: 2,
    selectElement: selectEl,
    showAllValues: true
})