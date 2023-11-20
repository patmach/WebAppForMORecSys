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

function rate(newValue, itemId, url) {
    $.ajax({
        url: url,
        cache: false,
        success: function (data) {
            if (data == 'MinimalPositiveRatingsDone') {
                let message = 'You have given the minimum required number of positive ratings.' +
                    '\nIf you press "OK", you will be redirected to home page to get recommendations.' +
                    '\nYou will still be able to rate other movies which will lead to better recommendations.' +
                    '\n\nIf you press "Cancel", you will stay on this page. You can start getting recommendations later by navigating to the home page.'
                if (confirm(message) == true) {
                    window.location.replace(window.location.origin);
                }
            }
        }
    });
    show_new_rating_value(newValue, itemId);
}

function unrate(itemId, url) {
    $.ajax({
        url: url,
        cache: false
    });
    show_new_rating_value(0, itemId);
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

/* Checking if the element can be seen partially by user*/
function isElementInView(element) {
    var pageTop = $(window).scrollTop();
    var pageBottom = pageTop + $(window).height();
    var elementTop = $(element).offset().top;
    var elementBottom = elementTop + $(element).height();
    return ((pageTop < elementTop) && ((pageBottom + ($(element).height() / 2)) > elementBottom));
}


function SaveItemPreviewSeenInteractions() {
    var itempreviews = $('.ItemPreview')
    for (let i = 0; i < itempreviews.length; i++) {
        if (seen[i]) continue;
        var itempreviews_top = itempreviews[i].getBoundingClientRect().top;
        var itempreviews_bottom = itempreviews[i].getBoundingClientRect().bottom;
        if (isElementInView(itempreviews[i])) {
            seen[i] = true;
            var id = itempreviews[i].id.replace('preview_', '');
            $.ajax(
                {
                    url: "Home/SetInteraction",
                    type: "POST",
                    dataType: "json",
                    data: { id: id, type: 1 }
                }
                );
        }      
    }
}

function setItemPreviewPopover() {
    popoverExplanationOptions = {
        content: function () {
            var classname = '.popover-content' + this.id;
            return $(this).siblings(classname).html();
        },
        trigger: 'hover',
        animation: true,
        placement: 'top',
        html: true,
        title: 'Why is this item recommended?'
    };
    $('.ItemPreview').popover(popoverExplanationOptions)
}

function checkIfUserStudyIsAllowed() {
    $.ajax({
        url: "Home/CheckIfUserStudyAllowed",
        cache: false,
        success: function (data) {
            if (data == true) {
                $('#gotoformcard')[0].style.display = '';
                $('#gotoformcard')[0].style.zIndex = '200';
                $('#tipcard')[0].style.position = 'relative';
            }
            else {
                $('#gotoformcard')[0].style.display = 'none';
            }
        },
        error: function (data) {
            $('#gotoformcard')[0].style.display = 'none';
        }
    });
}
