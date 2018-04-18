var index = 0;
var files = [/*"http://localhost:8000/DisplayName.txt",*/ "http://localhost:8000/Elo.txt", "http://localhost:8000/Rank.txt", "http://localhost:8000/GamesPlayed.txt", "http://localhost:8000/EloChange.txt", "http://localhost:8000/WinStreak.txt", "http://localhost:8000/DailyKills.txt", "http://localhost:8000/DailyGames.txt", "http://localhost:8000/DailyEloChange.txt", "http://localhost:8000/DailyWinCount.txt"];
var allText = "";

$(document).ready(function()
{
	loop();
	setInterval(function()
	{
		loop();
	}, 15000);
});

function loop()
{
	jQuery.get(files[index++], function(data)
	{
		allText = data;
		var time = Date.now();
		document.getElementById("testId").innerHTML = allText;
		$("#testId").fadeIn(3000, function()
		{
			var delta = 12000 - (Date.now() - time);
			$("#testId").delay(delta).fadeOut(3000);
		});
	});
	if(index >= files.length)
	{
		index = 0;
	}
}