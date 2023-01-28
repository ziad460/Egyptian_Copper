function getOptions(isFilter) {
    return {
        enableFiltering: isFilter,
        enableCaseInsensitiveFiltering: isFilter,
        filterPlaceholder: 'Search ...',
        class : "form-control text-center fw-bold"
    }
}
$('#year').multiselect(getOptions(true));