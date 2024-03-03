﻿
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
   dataTable = $('#tblData').DataTable({
        "ajax": { url : '/admin/product/getall' },
        "columns": [
         { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'author', "width": "20%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'category.name', "width": "15%" }
         ]
    });
}