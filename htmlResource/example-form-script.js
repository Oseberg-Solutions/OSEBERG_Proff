var Example = window.Example || {};

Example.formOnLoad = function (executionContext) {
  console.log("Form on load!!!");
  setTimeout(function () {
    console.log("Form on load2!!!");
    Start(executionContext);
  }, 5000);
};

window.Example = Example;

function Start(executionContext) {
  console.log("!!!!START!!!");
  var formContext = executionContext.getFormContext();

  //var webResourceControl = formContext.getControl("WebResource_new_1");

  /*
  var data = {
    orgNr: formContext.getAttribute("cr41c_orgnr").getValue(),
    name: formContext.getAttribute("name").getValue(),
    score: "C++",
    scorePoint: 34,
  };
  */

  var webResourceIframe = document.getElementById("WebResource_new_1"); // The ID of your iframe
  if (webResourceIframe) {
    var targetOrigin = window.location.origin; // Replace this with the origin of your web resource
    var message = {
      action: "update",
      data: { score: "D++", scorePoint: 34 },
    };
    webResourceIframe.contentWindow.postMessage(message, targetOrigin);
  }
}

function fetchApiAndSetValues(orgNr) {
  const baseURL = "azure.com";
  fetch(`${baseURL}?orgNr={orgNr}`)
    .then((response) => response.json())
    .then((data) => {
      console.log("Response-Data: ", data);
      updateScoreAndCircle(data.score, data.ScorePoint);
    })
    .catch((error) => console.error("Error fetching data:", error));
}

/*
<div id="rating-container" class="rating-container">
  <div id="score" class="score">A++</div>
  <div id="scorePoint" class="ScorePoint">99</div>
  <div id="ratingScore" class="ratingScore">
  <div id="scoreText" class="scoreText">Score: </div>
  <div id="scoreMaxPoint" class="scoreMaxPoint">/100</div>
</div>
*/

function updateScoreAndCircle(data) {
  console.log("Update Score and Circle: ", data);
  var iframe = document.querySelector(
    'iframe[src*="/webresources/os_proff_rating"]'
  );

  console.log("IFrame: ", iframe.contentWindow.document.body);

  iframe.contentWindow.document.getElementById("score").innerHTML = data.score;
  iframe.contentWindow.document.getElementById("scorePoint").innerHTML =
    data.scorePoint;
  iframe.contentWindow.document.getElementById("ratingScore").innerHTML =
    data.scorePoint;

  moveCircle(data.scorePoint);
}

function updateCircleBorderColor(iframe, circle, leftPosition) {
  var gaugeSections =
    iframe.contentWindow.document.querySelectorAll(".gauge-section");
  var colors = ["#ff6666", "#fbb72a", "#4bae49"]; // Colors corresponding to sections
  var sectionWidth = 100 / gaugeSections.length; // Assuming equal width sections

  var sectionIndex = Math.min(
    gaugeSections.length - 1,
    Math.floor(leftPosition / sectionWidth)
  );

  circle.style.borderColor = colors[sectionIndex];
}

// You might not need the example call to moveCircle here,
// as it will be called from updateScoreAndCircle
// moveCircle(80);
