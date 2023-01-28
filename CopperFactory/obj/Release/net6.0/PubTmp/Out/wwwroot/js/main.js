(function ($) {
    "use strict";
   
    /*Input-Password*/
    var showPass = 0;
    $('.pass-icon').on('click', function(){
        if(showPass == 0) {
            $(this).next('input').attr('type','text');
            $(this).find('i').removeClass('fa-eye');
            $(this).find('i').addClass('fa-eye-slash');
            showPass = 1;
        }
        else {
            $(this).next('input').attr('type','password');
            $(this).find('i').removeClass('fa-eye-slash');
            $(this).find('i').addClass('fa-eye');
            showPass = 0;
        }
        
    });

})(jQuery);

$(function() {

    var start = moment();
    var end = moment();

    function cb(start, end) {
        $('.reportrange span').html(start.format('DD/MM/YYYY') + ' - ' + end.format('DD/MM/YYYY'));
    }

    $('.reportrange').daterangepicker({
        startDate: start,
        endDate: end,
        ranges: {
           'All Time': [moment().subtract(30, 'years'), moment()],
           'Day': [moment(), moment()],
           'Month': [moment().subtract(30, 'days'), moment()],
           'year': [moment().subtract(1, 'year'), moment()],
        }
    }, cb);

    cb(start, end);

    showDropdowns = true;

});

$(function() {

    $('input[name="datefilter"]').daterangepicker({
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        }
    });
  
    $('input[name="datefilter"]').on('apply.daterangepicker', function(ev, picker) {
        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    });
  
    $('input[name="datefilter"]').on('cancel.daterangepicker', function(ev, picker) {
        $(this).val('');
    });
  
  });

  (function($) {

	"use strict";

	 $(document).ready(function() {
        $('#multiple-checkboxes').multiselect({
          includeSelectAllOption: true,
        });
    });
	 
  })(jQuery);

function openNav() {
    document.getElementById("mySidenav").style.width = "250px";
}

function closeNav() {
    document.getElementById("mySidenav").style.width = "0";
}

function makeActive(element) {
    if (element.classList.contains("active")) {
        element.classList.remove("active");
    }
    else {
        element.classList.add("active");
    }
}
