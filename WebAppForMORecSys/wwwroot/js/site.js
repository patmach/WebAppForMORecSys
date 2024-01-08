// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/** For closing sidebar filter */
function openNav() {
    var sidebar_width = "25%";
    if (window.innerWidth < 768) {
        sidebar_width = "50%"
    }
    $("#mySidebar")[0].style.width = sidebar_width;
    $("#main")[0].style.marginLeft = sidebar_width;
    $("#mySidebar")[0].style.display = "";
    $("#openfilterbtn")[0].style.display = "none";
    $("#closefilterbtn")[0].style.display = "";

}
/** For showing sidebar filter*/
function closeNav() {
    $("#mySidebar")[0].style.width = "0";
    $("#mySidebar")[0].style.display = "none";
    $("#main")[0].style.marginLeft = "0";
    $("#closefilterbtn")[0].style.display = "none";
    $("#openfilterbtn")[0].style.display = "";
}


/**
 * Sets possible values retrieved from the url for autocompletion of searched text in textbox.
 * @param {Object} textbox - Textbox html element that will provide autocomplete function
 * @param {string} url - URL where the data for autocompletion can be retrieved
 */
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

var x_coord_main_page = 0;
var y_coord_main_page = 0;

/**
 * Sets onclick event for each preview of the item (with css class ajaxdetail) that will show detail of the item.
 */
function setPreviewClick(){
    $(".ajaxdetail").on("click", function (e) {
        e.preventDefault();
        x_coord_main_page = window.scrollX;
        y_coord_main_page = window.scrollY;
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
                var arr = details.getElementsByTagName('script');
                for (var n = 0; n < arr.length; n++)
                    eval(arr[n].innerHTML)//run script inside div
                coords = getElementCoords(details);
                window.scrollTo(coords.left, coords.top);
            }
        });
    });
}

$(document).ready(function () {
    setPreviewClick();
});

/**
 * @param {Object} el - HTML element displayed on the page
 * @returns - Coordinates of the HTML element
 */
function getElementCoords(el) {
    const rect = el.getBoundingClientRect();
    return {
        left: rect.left + window.scrollX,
        top: rect.top + window.scrollY
    };
}

/**
 * Return from the details of the item to the main page withou reloading the main page contents.
 */
function BackToList() {
    document.getElementById('DetailsDiv').style.display = "none";
    document.getElementById('DetailsDiv').innerHTML = "";
    document.getElementById("Previews").style.display = "";
    document.getElementById('loadmore_div').style.display = "";
    window.scrollTo(x_coord_main_page, y_coord_main_page);
}

/**
 * After user blocks any item, its preview is shadowed and the icon for block is substitued by an icon for show again
 * @param {number} itemId - Id of the item
 */
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

/**
 * After user unblocks any item, its preview is displayed without shadow and the icon for show again is substitued by an icon for block
 * @param {number} itemId - Id of the item
 */
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

/**
* After user blocks value of any property, the icon for block is substitued by an icon for show again
 * @param {string} name - name of the property
 * @param {number} index - index of the value
 */
function hide_by_property(name, index) {
    hidelink = $('.hide' + name + '_' + index);
    showlink = $('.show' + name + '_' + index);
    showlink[0].style.display = "";
    hidelink[0].style.display = "none"
}

/**
* After user blocks value of any property, the icon for show is substitued by an icon for block
 * @param {string} name - name of the property
 * @param {number} index - index of the value
 */
function show_by_property(name, index) {
    hidelink = $('.hide' + name + '_' + index);
    showlink = $('.show' + name + '_' + index);
    showlink[0].style.display = "none";
    hidelink[0].style.display = ""
}

/**
 * After user changed its rating, the displayed rating is changed to be corresponding with the new value
 * @param {number} newValue - new value of the rating
 * @param {number} itemId - ID of the rated item
 */
function show_new_rating_value(newValue, itemId) {
    var ratingValue = newValue
    var startlist = $('.ratingstar' + itemId);
    while (startlist.length > 0) {
        for (var i = 0; i < ratingValue; i++) {
            startlist[i].classList.remove('fa-star-o');
            startlist[i].classList.add('fa-star');
        }
        for (var i = ratingValue; i < 10; i++) {
            startlist[i].classList.remove('fa-star');
            startlist[i].classList.add('fa-star-o');
        }
        startlist = startlist.slice(10);
    }
    var removeLinks = $('.remove' + itemId);
    for (var i = 0; i < removeLinks.length; i++) {
        if (newValue > 0) {
            removeLinks[i].style.display = '';
        }
        else {
            removeLinks[i].style.display = 'none';
        }
    }
}

/**
 * After user rates item, his new rating is saved and the displayed rating is changed to be corresponding with the new value
 * @param {number} newValue - new value of the rating
 * @param {number} itemId - ID of the rated item
 * @param {string} url - URL of method that will save the rating
 */
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

/**
 * After user removes rating of the item, his rating is deleted and the displayed rating is removed
 * @param {number} itemId - ID of the unrated item 
 * @param {string} url - URL of method that will remove the rating
 */
function unrate(itemId, url) {
    $.ajax({
        url: url,
        cache: false
    });
    show_new_rating_value(0, itemId);
}

/**
 * Close the card with information
 * @param {object} element_id - ID of the button that hides the info card
 */
function close_infocard(element_id) {
    var infocard = document.querySelector('#'+element_id).closest('.infocard');
    infocard.style.display = 'none';
}


/**
 * Sets to the element "randompreviewofrated" preview of random item from the ones rated by user
 * @param {string} url - URL of method that will return preview of random item
 */
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

/**
 * Checking if the element can be seen partially by user
 * @param {object} element - HTML element
 * @returns True - More than half of the element can be seen by user, False - otherwise
 */
function isElementInView(element) {
    var pageTop = $(window).scrollTop();
    var pageBottom = pageTop + $(window).height();
    var elementTop = $(element).offset().top;
    var elementBottom = elementTop + $(element).height();
    return ((pageTop < elementTop) && ((pageBottom + ($(element).height() / 2)) > elementBottom));
}

/**
 * Should be called after each scroll when neede to save all "seen" interactions between user and item
 */
function SaveItemPreviewSeenInteractions() {
    var itempreviews = $('.ItemPreview')
    for (let i = 0; i < itempreviews.length; i++) {
        if (seen[i]) continue;
        if (isElementInView(itempreviews[i])) {
            seen[i] = true;
            var id = itempreviews[i].id.replace('preview_', '');
            $.ajax(
                {
                    url: "Interactions/Save",
                    type: "POST",
                    dataType: "json",
                    data: { id: id, type: 1 }
                }
                );
        }      
    }
}

/**
 * Sets for each element with class "ItemPreview" corresponding popover content
 */
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

/**
 * Checks id user can go to the questionnaire. Potentionally displayes card with that information.
 */
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


//USER STUDY FORM
/**
 * Redirects to the following section and displays its questions
 */
function nextSection() {
    checkAnswersAndChangeSection(currentIndex + 1)
}

/**
 * Redirects to the preceding section and displays its questions
 */
function previousSection() {
    changeSection(currentIndex - 1)
}

/**
 * @param {object} element - div element containing the question
 * @returns id of question from id of div element containing the question
 */
function getQuestionIdFromQuestionDiv(element) {
    return parseInt(element.id.replace("question_", ""));
}


/*
function setImgTooltips() {
    var tooltips = $(".tooltip-with-img");
    for (let i = 0; i < tooltips.length; i++) {
        let img_id = tooltips[i].id.slice(0, -1) + 'img';
        let content = $('#' + img_id)[0].outerHTML;
        $('#' + tooltips[i].id).tooltip({ content: content });
    }
}
*/

// METRICS FILTER
/* Make sliders responsive to change of value */

/* Computes sum of values set to the merics*/
function getSumOfRangeValues() {
    let inputranges = document.getElementById("input-metrics-group").getElementsByTagName('input');
    let length = inputranges.length;
    let sum = 0;
    for (let i = 0; i < length; i++) {
        sum += parseInt(inputranges[i].value)
    }
    return sum;
}

function getRandomInt(max) {
    return Math.floor(Math.random() * max);
}

/**
 * When + and - buttons are used, the next method increases its value by 10
 */
function increase(index) {
    let inputranges = document.getElementById("input-metrics-group").getElementsByTagName('input');
    let value = parseInt(inputranges[index].value) + 10;
    value = Math.max(0, Math.min(100, value));
    inputranges[index].value = value;
    changeValues(value, index);
    setBackgroundOfBoxes();
}
/**
 * When + and - buttons are used, the next method decreases its value by 10
 */
function decrease(index) {
    let inputranges = document.getElementById("input-metrics-group").getElementsByTagName('input');
    let value = parseInt(inputranges[index].value) - 10;
    value = Math.max(0, Math.min(100, value));
    inputranges[index].value = value;
    changeValues(value, index);
    setBackgroundOfBoxes();
}




/**
 * Load metrics importance values to filter form. 
 * So the importance values are part of the request even if user uses the more detailed filter 
 * @param {Array<Object>} inputranges - set metrics importance values
 */
function copyValuesToOtherForm(inputranges) {
    let group = document.getElementById("input-copy-metrics-group");
    if (group === null) return;
    let inputhiddens = document.getElementById("input-copy-metrics-group").getElementsByTagName('input');
    if ((inputhiddens == null) || (inputhiddens.length == 0)) return;
    for (let i = 0; i < inputhiddens.length; i++) {
        inputhiddens[i].value = inputranges[i].value;
    }

}
/**Shows values under sliders. */
function setBubbles() {
    const allRanges = document.querySelectorAll(".range-wrap");
    allRanges.forEach(wrap => {
        const range = wrap.querySelector(".range");
        const bubble = wrap.querySelector(".bubble");
        if (bubble != null) {
            range.addEventListener("input", () => {
                setBubble(range, bubble);
            });
            setBubble(range, bubble);
        }
    });
}

/**
 * Set values under one slider
 * @param {object} range - HTML element of input of type range
 * @param {object} bubble - HTML element of bubble under the input of type range
 */
function setBubble(range, bubble) {
    const val = range.value;
    const min = range.min ? range.min : 0;
    const max = range.max ? range.max : 100;
    const newVal = Number(((val - min) * 100) / (max - min));
    bubble.innerHTML = val;
    // Sorta magic numbers based on size of the native UI thumb
    bubble.style.left = `calc(${newVal}% + (${8 - newVal * 0.15}px))`;
}

/**
 *  Change the colour transition in background and colour of text when using plus minus buttons. 
 */
function setBackgroundOfBoxes() {
    let boxes = document.getElementsByClassName("progressbox");
    let inputranges = document.getElementById("input-metrics-group").getElementsByTagName('input');
    if (boxes === null) return;
    for (let i = 0; i < boxes.length; i++) {
        boxes[i].style.background = 'linear-gradient(to right,' + colors[i] + ' '
            + (parseInt(inputranges[i].value) - 20) + '%, white ' + (parseInt(inputranges[i].value) + 20) + '% 100% ) ';
        if (parseInt(inputranges[i].value) > 50) {
            boxes[i].style.color = "white";
        }
        else {
            boxes[i].style.color = "black";
        }
    }
}


//Drag and drop

/** 
    * Sets the values corresponding to rank of the boxes.
    * Ratio is used. When there are 3 metrics. The sum of 100 is divided in 3:2:1
    */
function set_input_hidden_values() {
    //sets right values to hidden (metrics ranking is the same as on start)
    let inputranges = document.getElementById("input-metrics-group").getElementsByTagName('input');
    let length = inputranges.length;
    //let drag_ranges = document.getElementById("input-metrics-group").getElementsByClassName('drag');
    let numberOfParts = 0;
    for (let i = 0; i < length; i++) {
        numberOfParts += i + 1;
    }
    for (let i = 0; i < length; i++) {
        let index = parseInt(inputranges[i].id.replace(/^range/, ''));
        inputranges[index].value = 100 / numberOfParts * (length - i);
    }
    copyValuesToOtherForm(inputranges)
}

function handleDragStart(e) {
    this.style.opacity = '0.4';
    dragSrcEl = this;

    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/html', this.innerHTML);
}

function handleDragEnd(e) {

}

function handleDragOver(e) {
    e.preventDefault();
    return false;
}

function handleDragEnter(e) {
    this.classList.add('over');
}

function handleDragLeave(e) {
    this.classList.remove('over');
}


/**
    *  Order drag boxes of metrics when page is loaded. According to the rnaking user last used.
    */
function order_drag_boxes_at_start() {
    let drag_ranges = document.getElementById("input-metrics-group").getElementsByClassName('dragbox');
    if (drag_ranges.length == 0) {
        return;
    }
    let input_ranges = document.getElementById("input-metrics-group").getElementsByTagName('input');
    let temp = [];
    let values = [];
    let sorted = [];
    let backgrounds = [];
    for (let i = 0; i < drag_ranges.length; i++) {
        temp[i] = drag_ranges[i].innerHTML;
        backgrounds[i] = drag_ranges[i].style.backgroundColor
        values[i] = Number(input_ranges[i].value);
        sorted[i] = Number(input_ranges[i].value);

    }
    sorted.sort(function (a, b) { return a - b; });
    sorted = sorted.reverse();
    for (let i = 0; i < drag_ranges.length; i++) {
        let index = sorted.indexOf(values[i]);
        drag_ranges[index].innerHTML = temp[i];
        drag_ranges[index].style.backgroundColor = backgrounds[i]
        sorted[index] = -1; //next call of indexOf on the same value would return same index
    }
    set_input_hidden_values()
}



