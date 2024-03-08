// Call the dataTables jQuery plugin
$(document).ready(function() {
    $('#dataTable').DataTable({
        "language": {
            "url": '@Url.Content("~/Content/Vendor/datatables/json.json")'
        }
    });
});
