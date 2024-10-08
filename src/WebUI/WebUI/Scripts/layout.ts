const menuButton = document.getElementById('menubutton');
const offCanvasMenu = document.getElementById('offCanvasMenu');
const backdrop = document.getElementById('backdrop');
const closeMenu = document.getElementById('closeMenu');

const userMenuButton = document.getElementById('user-menu-button'); 

userMenuButton.addEventListener('click', () => {
    
});

menuButton.addEventListener('click', () => {
    offCanvasMenu.classList.remove('-translate-x-full');
    backdrop.classList.remove('hidden');
});

closeMenu.addEventListener('click', () => {
   offCanvasMenu.classList.add('-translate-x-full');
   backdrop.classList.add('hidden');
});

backdrop.addEventListener('click', () => {
   offCanvasMenu.classList.add('-translate-x-full');
   backdrop.classList.add('hidden');
});