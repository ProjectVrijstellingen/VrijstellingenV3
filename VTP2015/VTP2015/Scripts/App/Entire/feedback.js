$('html').click(function () {
    $('#feedbackWidget').hide();
})

$('#feedback').click(function (e) {
    e.preventDefault();
    e.stopPropagation();
});

$('#link').click(function (e) {
    $('#feedbackWidget').toggleClass();
});