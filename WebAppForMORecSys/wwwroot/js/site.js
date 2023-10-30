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

function SetAutoComplete(textbox, url)
{
    if ($("#" + textbox))
    {
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
}

var xcoord_b4_details = 0;
var ycoord_b4_details = 0;

function setPreviewClick(){
    $(".ajaxdetail").on("click", function (e) {
        e.preventDefault();
        xcoord_b4_details = window.scrollX;
        ycoord_b4_details = window.scrollY;
        var elementUrl = $(this).attr('id');
        $.ajax({
            url: elementUrl,
            cache: false,
            success: function (data) {
                var details = document.getElementById('DetailsDiv');
                details.innerHTML = data;
                details.style.display = ""
                document.getElementById('Previews').style.display = "none";
                document.getElementById('loadmore_div').style.display = "none";
                coords = getElementCoords(details);
                window.scrollTo(coords.left, coords.top);
            }
        });
    });
}

$(document).ready(function () {
    setPreviewClick();
});

function getElementCoords(el) {
    const rect = el.getBoundingClientRect();
    return {
        left: rect.left + window.scrollX,
        top: rect.top + window.scrollY
    };
}

function BackToList() {
    document.getElementById('DetailsDiv').style.display = "none";
    document.getElementById('DetailsDiv').innerHTML = "";
    document.getElementById("Previews").style.display = "";
    document.getElementById('loadmore_div').style.display = "";
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

function show_new_rating_value(newValue, itemId) {
    var ratingValue = newValue
    var startlist = $('.ratingstar' + itemId);
    for (var i = 0; i < ratingValue; i++) {
        startlist[i].classList.remove('fa-star-o');
        startlist[i].classList.add('fa-star');
    }
    for (var i = ratingValue; i < 10; i++) {
        startlist[i].classList.remove('fa-star');
        startlist[i].classList.add('fa-star-o');
    }
    var removeLink = $('.remove' + itemId)[0];
    if (newValue > 0) {
        removeLink.style.display = '';
    }
    else {
        removeLink.style.display = 'none';
    }
}

function close_infocard(element_id) {
    var infocard = document.querySelector('#'+element_id).closest('.infocard');
    infocard.style.display = 'none';
}

/*Preview of random rated item*/
function getRandomPreview(url) {
    if ($('#randompreviewofrated').length > 0) {
        $.ajax({
            url: url,
            cache: false,
            success: function (data) {
                $('#randompreviewofrated').html(data);
                $(".rating")[0].style.display = "none";
            },
        });
    }
}


