async function imageToBase64(imagePath) {
	const response = await fetch(imagePath);
	const blob = await response.blob();
	return new Promise((resolve, reject) => {
		const reader = new FileReader();
		reader.onloadend = () => resolve(reader.result);
		reader.onerror = reject;
		reader.readAsDataURL(blob);
	});
}

function ajaxGet(url, callback) {
    var ajax = new XMLHttpRequest();
    ajax.open("GET", url, true);
    ajax.setRequestHeader("Access-Control-Allow-Origin", "*");   
    ajax.setRequestHeader('Access-Control-Allow-Methods', '*');    
    ajax.setRequestHeader('Access-Control-Allow-Headers', 'x-requested-with,content-type');  
    ajax.send(null);

    ajax.onreadystatechange = function () {
        if (ajax.readyState === 4) {
            callback && callback(ajax.status, ajax.responseText);
        }
    }
}