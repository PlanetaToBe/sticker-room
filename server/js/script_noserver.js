////////////////////////////////////////////////////////////	
// SET_UP_VARIABLES
////////////////////////////////////////////////////////////

var scene, camera, container, renderer, effect, stats;
var vrmanager;
var hemiLight, controls;
var screenWidth = window.innerWidth;
var screenHeight = window.innerHeight;
var time, clock;

var loadedCount = 0;

var myStartX = 0, myStartZ = -10, myStartY = 0; //2
var myPosition, myStartRotY, worldBubble, pplCount, pplCountTex, pplCountMat;

var model, texture;
var dummy;
var perlin = new ImprovedNoise(), noiseQuality = 1;

var basedURL = "assets/";

var textureLoader, loadingManger, br_mat_loadingManager, starLoadingManager;
var poop_TLM, poopHeart_TLM, graffiti_TLM, floor_TLM, door_TLM, intestine_TLM;
var poster_TLM, waterwave_TLM, glow_TLM, person_TLM, skin_TLM, particle_TLM;
var keyIsPressed;

// RAYCAST
	var objects = [];
	var ray;
	var projector, eyerayCaster, eyeIntersects;
	var lookDummy, lookVector;

// PLAYERS
	var skinTexture;
	var guyBodyGeo, guyLAGeo, guyRAGeo, guyHeadGeo;
	var personTex;
	var player, playerBody, playerHead;
	var firstPlayer, secondPlayer;
	var firstGuy, firstGuyBody, firstGuyHead, secondGuy, secondGuyBody, secondGuyHead;
	var QforBodyRotation;
	var fGuyHandHigh = false, sGuyHandHigh = false;
	var bodyGeo;
	var dailyLifeME, colorME, dailyLifePlayers = [];
	var dailyLifePlayerDict = {};

	var person, personGeo, personMat, toiletTex, toiletMat;
	var persons = [], personIsWalking = [], personCircle, personAmount = 3;
	var personsAppeared = false, personsAnied = false, personsWalked = false;
	var personWalkTimeoutID, personAniInterval, personAniIntervalCounter=0, personAniSequence = [1,3,5,6];
	var poop, poopGeo, poopTex, poopMat, poopHat, poopHeartTex;
	var poopM, poopMGeo, poopMTex, poopMMat, poopHeartGeo, poopHeartMat, poopHeart;
	var personBody, personHead, personToilet;
	var keyframe, lastKeyframe, currentKeyframe;
	var animOffset = 1, keyduration = 28;
	var aniStep = 0, aniTime = 0, slowAni = 0.4;
	var personKeyframeSet =   [ 28, 15,  1,  8,  1, 12, 10, 1 ];
	var personAniOffsetSet = [  1, 30, 48, 50, 58, 60, 72, 82 ];	//2: sit freeze; 4: push freeze; 7: stand freeze
	var personFreeze = false;

//
	var planet;

////////////////////////////////////////////////////////////

// init();				// Init after CONNECTION
superInit();			// init automatically

///////////////////////////////////////////////////////////
// FUNCTIONS 
///////////////////////////////////////////////////////////
function superInit(){

	//Prevent scrolling for Mobile
	noScrolling = function(event){
		event.preventDefault();
	};

	time = Date.now();

	// THREE.JS -------------------------------------------
		clock = new THREE.Clock();

	// RENDERER
		container = document.getElementById('render-canvas');
		renderer = new THREE.WebGLRenderer({antialias: true, alpha: true});
		renderer.setPixelRatio(Math.floor(window.devicePixelRatio));
		renderer.setClearColor(0x34122a, 1);
		container.appendChild(renderer.domElement);

	// VR_EFFECT
		effect = new THREE.VREffect(renderer);
		effect.setSize(window.innerWidth, window.innerHeight);

	// Create a VR manager helper to enter and exit VR mode.
		var params = {
		  hideButton: false, // Default: false.
		  isUndistorted: false // Default: false.
		};
		vrmanager = new WebVRManager(renderer, effect, params);

	// SCENE
		scene = new THREE.Scene();

	// LIGHT
		hemiLight = new THREE.HemisphereLight( 0xf9ff91, 0x3ac5b9, 1);
		hemiLight.intensity = 1;
		scene.add(hemiLight);

	// CAMERA
		camera = new THREE.PerspectiveCamera(40, window.innerWidth / window.innerHeight, 0.1, 10000);
		camera.position.z -= 0.6;

	// RAYCASTER!
		eyerayCaster = new THREE.Raycaster();	

	loadModelTruck( basedURL + "models/foodCarts_small/cart_cart.json",
					basedURL + "models/foodCarts_small/cart_lantern.json",
					basedURL + "models/foodCarts_small/cart_rooftop.json",
					basedURL + "models/foodCarts_small/cart_supports.json",
					basedURL + "models/foodCarts_small/cart_wheels.json",
					basedURL + "models/foodCarts_small/cart_wood.json" );

	// planet = new THREE.Mesh( new THREE.BoxGeometry( 1,1,1 ), new THREE.MeshLambertMaterial({side: THREE.DoubleSide}) );
	// scene.add( planet );

	stats = new Stats();
	stats.domElement.style.position = 'absolute';
	stats.domElement.style.bottom = '5px';
	stats.domElement.style.zIndex = 100;
	stats.domElement.children[ 0 ].style.background = "transparent";
	stats.domElement.children[ 0 ].children[1].style.display = "none";
	container.appendChild( stats.domElement );
	
	// EVENTS
	window.addEventListener('resize', onWindowResize, false);

	lateInit();	
}

// lateInit() happens after click "Start"
function lateInit() 
{	
	// console.log("late init!");
	document.body.addEventListener('touchmove', noScrolling, false);
	window.addEventListener('keydown', myKeyPressed, false);
	window.addEventListener('keyup', myKeyUp, false);

	clock.start();

	myPosition = new THREE.Vector3( myStartX, myStartY, myStartZ );
	myWorldCenter = new THREE.Vector3();

	// create controls
	controls = new THREE.DeviceControls(camera, myWorldCenter, true);
	scene.add( controls.getObject() );

	// start to animate()!
	animate(performance ? performance.now() : Date.now());

	trulyFullyStart = true;
}

function myKeyPressed( event ){
	if(keyIsPressed)	return;
	keyIsPressed = true;

	switch ( event.keyCode ) {

		case 49: //1
			//
			break;
	}
}

function myKeyUp(event){
	keyIsPressed = false;
}

var lastRender = 0;
function animate(timestamp) {
	// if(!isAllOver){
		var delta = Math.min(timestamp - lastRender, 500);
		lastRender = timestamp;

		update();
		
		// Render the scene through the manager.
		vrmanager.render(scene, camera, timestamp);
		stats.update();
	// }

	requestAnimationFrame(animate);
}


function update()
{	
	controls.update( Date.now() - time );
	var dt = clock.getDelta();

	//
	time = Date.now();
}

function render() 
{	
	effect.render(scene, camera);
}

function fullscreen() {
	if (container.requestFullscreen) {
		container.requestFullscreen();
	} else if (container.msRequestFullscreen) {
		container.msRequestFullscreen();
	} else if (container.mozRequestFullScreen) {
		container.mozRequestFullScreen();
	} else if (container.webkitRequestFullscreen) {
		container.webkitRequestFullscreen();
	}
}

function onWindowResize() {
	effect.setSize( window.innerWidth, window.innerHeight );
	camera.aspect = window.innerWidth / window.innerHeight;
	camera.updateProjectionMatrix();
}

function isTouchDevice() { 
	return 'ontouchstart' in window || !!(navigator.msMaxTouchPoints);
}
