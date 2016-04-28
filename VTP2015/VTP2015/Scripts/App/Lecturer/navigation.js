$(document).ready(function () {
    $("#ShowBehandeld").on("click", toonGevraagde);
    $("#ShowVerwijder").on("click", toonGevraagde);
    $("#ShowOpen").on("click", toonGevraagde);
});


var toonGevraagde = function ()
{
    hideAll();
    var toonId = "#" + $(this).attr("id").substring(4);
    console.log(toonId);
    $(toonId).removeClass("hide");
}

var hideAll = function () {
    $("#Open").addClass("hide");
    $("#Behandeld").addClass("hide");
    $("#Verwijder").addClass("hide");
}