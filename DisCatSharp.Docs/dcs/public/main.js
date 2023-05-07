// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

$(function() {
    $('table').each(function(a, tbl) {
        var currentTableRows = $(tbl).find('tbody tr').length;
        $(tbl).find('th').each(function(i) {
            var remove = 0;
            var currentTable = $(this).parents('table');

            var tds = currentTable.find('tr td:nth-child(' + (i + 1) + ')');
            tds.each(function(j) { if ($(this).text().trim() === '') remove++; });

            if (remove == currentTableRows) {
                $(this).hide();
                tds.hide();
            }
        });
    });
});