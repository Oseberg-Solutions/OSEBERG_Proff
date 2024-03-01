var Example = window.Example || {};

Example.formOnLoad = function (executionContext) {
  console.log("Form on load!!!");

  var formContext = executionContext.getFormContext();

  var webResourceControl = formContext.getControl("WebResource_new_1");

  if (webResourceControl) {
    var data = {
      orgNr: formContext.getAttribute("cr41c_orgnr").getValue(),
      name: formContext.getAttribute("name").getValue(),
    };

    //fetchApiAndSetValues(data.orgNr);
  }
};

window.Example = Example;

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

function updateScoreAndCircle(iframe, data) {
  var iframe = document.querySelector(
    'iframe[src*="/webresources/os_proff_rating"]'
  );
  document.getElementById("score").innerHTML = data.score;
  document.getElementById("scorePoint").innerHTML = data.scorePoint;
  document.getElementById("ratingScore").innerHTML = data.scorePoint;
  document.getElementById("scoreText").innerHTML = data.scorePoint;
  document.getElementById("scoreMaxPoint").innerHTML = data.scorePoint;
  moveCircle(data.scorePoint);
}

function moveCircle(score) {
  // Your existing moveCircle function
}

function updateCircleBorderColor(circle, leftPosition) {
  // Your existing updateCircleBorderColor function
}

// You might not need the example call to moveCircle here,
// as it will be called from updateScoreAndCircle
// moveCircle(80);
