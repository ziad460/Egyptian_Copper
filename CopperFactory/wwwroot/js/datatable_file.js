$(document).ready(function () {
    var dataTable = $('#pageTable').DataTable({
        "columnDefs": [
            { "className": "dt-center", "targets": "_all" }
        ],
        /*"dom": 'lrt' */
    });
    $("#filter").keyup(function () {
        dataTable.search(this.value).draw();
    })
});
$(document).ready(function () {
    $('.pageSelect').select2({
        allowClear: true
    });
});
function Delete() {
    
    var result = confirm("Are you sure want to delete this item.....??");
    if (result == false) {
        return false;
    }
    else {
        return true;
    }
}

$('.numbers').keyup(function () {
    this.value = this.value.replace(/[^0-9\.]/g, '');
});

function tableToExcel() {
    var table2excel = new Table2Excel();
    table2excel.export(document.querySelectorAll("table.table"));
}
