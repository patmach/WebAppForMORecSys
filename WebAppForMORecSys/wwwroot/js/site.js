// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/* For showing and closing sidebar filter*/
function openNav() {
    document.getElementById("mySidebar").style.width = "25%";
    document.getElementById("main").style.marginLeft = "25%";
    document.getElementById("openfilterbtn").style.display = "none";
    document.getElementById("closefilterbtn").style.display = "";

}

function closeNav() {
    document.getElementById("mySidebar").style.width = "0";
    document.getElementById("main").style.marginLeft = "0";
    document.getElementById("closefilterbtn").style.display = "none";
    document.getElementById("openfilterbtn").style.display = "";
}

function SetAutoComplete(textbox, url) {
    $("#" + textbox).autocomplete(
        {
            source: function (request, response) {
                $.ajax(
                    {
                        url: url,
                        type: "POST",
                        dataType: "json",
                        data: { Prefix: request.term },
                        success: function (data) {
                            response($.map(data, function (item) {
                                return { label: item, value: item };
                            }))
                        }
                    })
            },
            messages: {
                noResults: 'No results found.',
                results: function (count) {
                    return count + (count > 1 ? ' results' : ' result ') + ' found';
                }
            }
        });
}

var xcoord_b4_details = 0;
var ycoord_b4_details = 0;
$(document).ready(function () {
    $(".ajaxdetail").on("click", function (e) {
        e.preventDefault();
        xcoord_b4_details = window.scrollX;
        ycoord_b4_details = window.scrollY;
        var elementUrl = $(this).attr('id');
        $.ajax({
            url: elementUrl,
            cache: false,
            success: function (data) {
                $('#DetailsDiv').html(data);
                document.getElementById("DetailsDiv").style.display = ""
                document.getElementById("Previews").style.display = "none";
                window.scrollTo(0, 0);
            }
        });
    });
});

function BackToList() {
    document.getElementById('DetailsDiv').style.display = "none";
    document.getElementById('DetailsDiv').innerHTML = "";
    document.getElementById("Previews").style.display = "";
    window.scrollTo(xcoord_b4_details, ycoord_b4_details);
}

function hide(itemId) {
    var list = $('.preview_' + itemId);
    list[0].style.opacity = 0.5;
    list[0].getElementsByClassName("hideimg")[0].style.display = "none";
    list[0].getElementsByClassName("showimg")[0].style.display = "";
    list = $('.Detail_' + itemId);
    if (list.length > 0) {
        list[0].style.opacity = 0.5;
        var x = list[0].getElementsByClassName("hideimg")[0];
        list[0].getElementsByClassName("hideimg")[0].style.display = "none";
        list[0].getElementsByClassName("showimg")[0].style.display = "";
    }
}

function show(itemId) {
    var list = $('.preview_' + itemId);
    list[0].style.opacity = 1;
    list[0].getElementsByClassName("showimg")[0].style.display = "none";
    list[0].getElementsByClassName("hideimg")[0].style.display = "";
    list = $('.Detail_' + itemId);
    if (list.length > 0) {
        list[0].style.opacity = 1;
        var x = list[0].getElementsByClassName("showimg")[0];
        list[0].getElementsByClassName("showimg")[0].style.display = "none";
        list[0].getElementsByClassName("hideimg")[0].style.display = "";
    }
}

function hide_by_property(name, index) {
    hidelink = $('.hide' + name + '_' + index);
    showlink = $('.show' + name + '_' + index);
    showlink[0].style.display = "";
    hidelink[0].style.display = "none"
}

function show_by_property(name, index) {
    hidelink = $('.hide' + name + '_' + index);
    showlink = $('.show' + name + '_' + index);
    showlink[0].style.display = "none";
    hidelink[0].style.display = ""
}