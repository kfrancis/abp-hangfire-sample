var _jobNotificationTimer = null;
$(function () {
    var l = abp.localization.getResource('BookStore');

    var dataTable = $('#BooksTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(acme.bookStore.books.book.getList),
            columnDefs: [
                {
                    title: l('Id'),
                    data: "id"
                },
                {
                    title: l('Name'),
                    data: "name"
                },
                {
                    title: l('Type'),
                    data: "type",
                    render: function (data) {
                        return l('Enum:BookType.' + data);
                    }
                },
                {
                    title: l('PublishDate'),
                    data: "publishDate",
                    render: function (data) {
                        return luxon
                            .DateTime
                            .fromISO(data, {
                                locale: abp.localization.currentCulture.name
                            }).toLocaleString();
                    }
                },
                {
                    title: l('Price'),
                    data: "price"
                },
                {
                    title: l('CreationTime'), data: "creationTime",
                    render: function (data) {
                        return luxon
                            .DateTime
                            .fromISO(data, {
                                locale: abp.localization.currentCulture.name
                            }).toLocaleString(luxon.DateTime.DATETIME_SHORT);
                    }
                }
            ]
        })
    );

    $('#BooksTable tbody').on('click', 'tr', function () {
        var data = dataTable.row(this).data();
        $('#ReportContent').val(data.id);
    });

    var connection = new signalR.HubConnectionBuilder().withUrl("/my-messaging/background-jobs").build();

    

    connection.on('backgroundJobMessage', function (eventData) { // Register for incoming messages
        var eventDataSplit = eventData.split(";");
        var userId = eventDataSplit[0];
        var tenantId = eventDataSplit[1];

        // don't notify yourself
        var eventName = eventDataSplit[2].toUpperCase();
        var eventItem = JSON.parse(eventDataSplit[3]);

        switch (eventName) {

            case "App.ReportComplete".toUpperCase():

                if (_jobNotificationTimer != null) {
                    clearTimeout(_jobNotificationTimer);
                    _jobNotificationTimer = null;
                }
                _jobNotificationTimer = setTimeout(function () {
                    abp.notify.info(l("ReportCompleted", eventItem.Message));
                }, 1000);
                break;
        }

    });

    connection.start().then(function () {
        abp.log.debug('Connected to background events server!');
        connection.invoke('RegisterConnectionId', abp.currentUser.id);
    }).catch(function (err) {
        return console.error(err.toString());
    });
});
