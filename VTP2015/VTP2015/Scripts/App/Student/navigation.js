//pas refactoren als alle views goed staan.
$(document).ready(function () {
    showPrevProgress();
    showCurrentPage();
});

//hoofd nav bar
$("#start > a").click(function () {
    toonStap(0);
})

//buttons
$("#btnStap0").click(function () {
    toonStap(1);
    $("#navStap1").parent().removeClass('hide');
    
});
$("#btnStap1").click(function () {
    toonStap(2);
    $("#navStap2").parent().removeClass('hide');
   
});
$("#btnStap2").click(function () {
    toonStap(3);
    $("#navStap3").parent().removeClass('hide');
  
});
$("#btnStap3").click(function () {
    toonStap(4);
    $("#navStap4").parent().removeClass('hide');
});
$("#btnStap4").click(function () {
    toonStap(5);
    $("#navStap5").parent().removeClass('hide');
    
});
$("#btnStap5").click(function () {
    toonStap(6);
    $("#navStap6").parent().removeClass('hide');
 
});
$("#btnStap6").click(function () {
    toonStap(7);
    $("#navStap7").parent().removeClass('hide');
  
});


//nav bars
$("#navStap1").click(function (e) {
    e.preventDefault();
    toonStap(1);
});
$("#navStap2").click(function (e) {
    e.preventDefault();
    toonStap(2);
});
$("#navStap3").click(function (e) {
    e.preventDefault();
    toonStap(3);
});
$("#navStap4").click(function (e) {
    e.preventDefault();
    toonStap(4);
});
$("#navStap5").click(function (e) {
    e.preventDefault();
    toonStap(5);
});
$("#navStap6").click(function (e) {
    e.preventDefault();
    toonStap(6);
});
$("#navStap7").click(function (e) {
    e.preventDefault();
    toonStap(7);
});
$("#DossierOk").click(function () {
    localStorage.removeItem('currentStap');
    localStorage.removeItem('lastStep');
});


var setCurrentPage = function(nr){
    localStorage.currentStap = nr;
    showCurrentPage();
}

var showCurrentPage = function () {
    VerbergAllStappen(7);
    $("#stap" + localStorage.currentStap).removeClass('hide');
  
};
    

var toonStap = function (nr) {
    setCurrentPage(nr);
    if (nr > localStorage.getItem("lastStep")) {
        localStorage.lastStep = nr;
    }
}

var VerbergAllStappen = function (AantalStappen) {
    for (var i = 0; i <= AantalStappen; i++) {
        $("#stap" + i).addClass('hide');
      
    }
}

var showPrevProgress = function () {
    if (!localStorage.currentStap) {
        setCurrentPage(0);
    }
    if (!localStorage.currentStap) {
        localStorage.lastStep = 0;
    }
    var prevProgress = localStorage.getItem("lastStep")
    for (var i = 0; i <= prevProgress; i++) {
        $("#navStap" + i).parent().removeClass('hide');
    }
}
