// to get current year
function getYear() {
    var currentDate = new Date();
    var currentYear = currentDate.getFullYear();
    document.querySelector("#displayYear").innerHTML = currentYear;
}

getYear();


// isotope js
$(window).on('load', function () {

    $('.filters_menu li').click(function () {
        $('.filters_menu li').removeClass('active');
        $(this).addClass('active');

        var data = $(this).attr('data-filter');

        $grid.isotope({
            filter: data,
            sortBy: 'price',
            sortAscending: true
        })
    });

    var $grid = $(".grid").isotope({
        itemSelector: ".all",
        percentPosition: false,
        masonry: {
            columnWidth: ".all"
        },
        getSortData: {
            price: function (element) {
                var priceText = $(element).find('.price').text();
                return parseFloat(priceText.replace(/[^\d.-]/g, '')); // Extract numerical value
            }
        }
    })
    // Lắng nghe sự kiện nhập dữ liệu vào ô tìm kiếm
    $('#searchInput').on('input', function () {
        var searchText = $(this).val().toLowerCase();

        if (searchText == "") {
            $('.filters_menu li').removeClass('active');
            $('.filters_menu li[data-filter="*"]').addClass('active'); // Kích hoạt phần tử All
            $('#sortSelect').val($('#sortSelect option:first').val());
            // Nếu rỗng, đặt lại filter về mục "all"
            $grid.isotope({
                filter: ".all",
                sortBy: 'price',
                sortAscending: true
            });
        }
        else {
            $grid.isotope({
                filter: function () {
                    var $element = $(this);
                    var isVisible = $element.is(":visible"); // Check if the element is visible
                    if (!isVisible) {
                        return false; // Exclude hidden elements from the search
                    }

                    var itemText = $(this).find('h5, p').text().toLowerCase(); // Lấy nội dung của h5 và p
                    return itemText.indexOf(searchText) !== -1;
                }
            });
        }
        
    });
    $('#sortSelect').change(function () {
        var sortValue = $(this).val();

        if (sortValue === 'asc') {
            $grid.isotope({ sortBy: 'price', sortAscending: true });
        } else if (sortValue === 'desc') {
            $grid.isotope({ sortBy: 'price', sortAscending: false });
        }
    });

});

// nice select
$(document).ready(function() {
    $('select').niceSelect();
  });

/** google_map js **/
function myMap() {

    //var mapProp = {
    //    center: new google.maps.LatLng(40.712775, -74.005973),
    //    zoom: 18,
    //};
    //var map = new google.maps.Map(document.getElementById("googleMap"), mapProp);
}

// client section owl carousel
$(".client_owl-carousel").owlCarousel({
    loop: true,
    margin: 0,
    dots: false,
    nav: true,
    navText: [],
    autoplay: true,
    autoplayHoverPause: true,
    navText: [
        '<i class="fa fa-angle-left" aria-hidden="true"></i>',
        '<i class="fa fa-angle-right" aria-hidden="true"></i>'
    ],
    responsive: {
        0: {
            items: 1
        },
        768: {
            items: 2
        },
        1000: {
            items: 2
        }
    }
});

