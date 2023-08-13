/// <reference path="libs/js/action.js" />
/// <reference path="libs/js/stream-deck.js" />
/// <reference path="utils.js" />

const microzoneActions = {};

class ValorantAssistantAction {
    constructor (context, payload) {
        this.context = context;
        this.payload = payload;
        this.interval = null;
        this.init();
        this.update();
    }

    init() {
		this.activeImage = null;
		this.deactiveImage = null;
		this.serviceOnline = false;
		this.valorantIsRunning = false;

        imageToBase64('assets/images/valorant-on.png')
			.then((imgBase64) => {
				this.activeImage = imgBase64;
			})
			.catch((error) => {
				console.error('Error converting image to base64:', error);
			});

		imageToBase64('assets/images/valorant-off.png')
			.then((imgBase64) => {
				this.deactiveImage = imgBase64;
			})
			.catch((error) => {
				console.error('Error converting image to base64:', error);
			});

        this.interval = setInterval(() => {
            this.update();
        }, 1000);
    }

	update() {
		if(this.context === undefined || this.context === null)
			return;
		
		if(!this.serviceOnline)
		{
			ajaxGet("http://localhost:22333/api/Status", (code, text)=>{
				if(code != 200)
					return;
				let respone = JSON.parse(text == "" ? '{}' : text);
				if(respone.data === undefined || respone.data == null || typeof respone.data !== 'boolean')
					return;
				this.serviceOnline = respone.data;
			});
		}
		else
		{
			ajaxGet("http://localhost:22333/api/Valorant", (code, text)=>{
				if(code != 200)
					return;
				let respone = JSON.parse(text == "" ? '{}' : text);
				if(respone.data === undefined || respone.data == null || typeof respone.data !== 'boolean')
					return;
				this.valorantIsRunning = respone.data;
			});
		}

		if(this.valorantIsRunning)
		{
			if(this.activeImage !== undefined && this.activeImage !== null && typeof this.activeImage === 'string')
        		$SD.setImage(this.context, this.activeImage);
		}
		else
		{
			if(this.deactiveImage !== undefined && this.deactiveImage !== null && typeof this.deactiveImage === 'string')
        		$SD.setImage(this.context, this.deactiveImage);
		}
    }

	stopValorant() {
		if(!this.serviceOnline)
			return;

		ajaxGet("http://localhost:22333/api/Valorant/stop");
	}
};

const microzoneAction = new Action('com.kiro.microzone.valorant-assistant');

microzoneAction.onWillAppear(({context, payload}) => {
    microzoneActions[context] = new ValorantAssistantAction(context, payload);
});

microzoneAction.onWillDisappear(({context}) => {
    microzoneActions[context].interval && clearInterval(microzoneActions[context].interval);
    delete microzoneActions[context];
});

microzoneAction.onKeyUp(({ action, context, device, event, payload }) => {
	microzoneActions[context].stopValorant()
});