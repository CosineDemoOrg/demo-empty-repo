(() => {
  // ---- CONFIG ----
  const GAME_WIDTH = 900, GAME_HEIGHT = 540;
  const GROUND_HEIGHT = 120;
  const UNICORN_Y = GAME_HEIGHT - GROUND_HEIGHT - 24;
  const GRAVITY = 1100;
  const JUMP_VELOCITY = 470;
  const JUMP_HOLD_BOOST = 140;
  const SCROLL_MIN = 180;
  const SCROLL_MAX = 450;
  const SCROLL_START = 260;
  const OBSTACLE_MIN = 1500, OBSTACLE_MAX = 2200;
  const STAR_MIN = 1800, STAR_MAX = 2400;
  const UNICORN_X = 140;
  const STAR_SCORE = 10;

  // ---- CDN SOUNDS ----
  const MUSIC_URL = 'https://cdn.pixabay.com/audio/2022/12/19/audio_12e6f6ca8a.mp3'; // "Happy Adventure" (Pixabay, ~80KB)
  const JUMP_URL = 'https://cdn.pixabay.com/audio/2022/05/25/audio_120a4e6b7a.mp3'; // "Horse Whinny" (Pixabay, ~12KB)

  class GameScene extends Phaser.Scene {
    constructor() {
      super('GameScene');
      this.scrollSpeed = SCROLL_START;
      this.maxDistance = 0;
    }

    preload() {
      // --- Unicorn gallop frames ---
      this.createUnicornFrames();

      // --- Obstacle ("cloud") ---
      this.createObstacleFrame();

      // --- Star (rainbow 5-point) ---
      this.createStarFrame();

      // Sounds
      this.load.audio('bgm', MUSIC_URL);
      this.load.audio('jump', JUMP_URL);
    }

    create() {
      // Camera/physics setup
      this.physics.world.setBounds(0, 0, 1e6, GAME_HEIGHT);
      this.cameras.main.setBackgroundColor('rgba(0,0,0,0)');

      // Ground: rainbow tile
      this.ground = this.add.tileSprite(0, GAME_HEIGHT - GROUND_HEIGHT, GAME_WIDTH * 2, GROUND_HEIGHT, null)
        .setOrigin(0,0);
      this.drawRainbowGround(this.ground);

      // Unicorn
      this.unicorn = this.physics.add.sprite(UNICORN_X, UNICORN_Y, 'unicorn0').setOrigin(0.5,1);
      this.unicorn.setCollideWorldBounds(false);
      this.unicorn.body.setSize(56, 68).setOffset(2,9);

      this.anims.create({
        key: 'run',
        frames: [{key:'unicorn0'},{key:'unicorn1'}],
        frameRate: 10,
        repeat: -1
      });
      this.unicorn.play('run');

      // Physics
      this.unicorn.body.setGravityY(GRAVITY);

      // Obstacles group
      this.obstacles = this.physics.add.group();

      // Stars group
      this.stars = this.physics.add.group();

      // Collisions
      this.physics.add.collider(this.unicorn, this.obstacles, this.gameOver, null, this);
      this.physics.add.overlap(this.unicorn, this.stars, this.collectStar, null, this);

      // Score
      this.score = 0;
      this.distance = 0;
      this.scoreText = this.add.text(28, 18, 'Score: 0', {
        fontFamily: '"Comfortaa", sans-serif',
        fontSize: '32px',
        fontStyle: 'bold',
        color: '#fff',
        stroke: '#b824ff',
        strokeThickness: 4,
        shadow: { offsetX:2, offsetY:2, color:'#000', blur:1, fill:true }
      }).setDepth(10).setScrollFactor(0);

      // Max distance for scoreboard
      this.maxDistance = Number(localStorage.getItem('unicorn_best') || 0);

      // Overlay (Game Over)
      this.overlay = this.add.dom(GAME_WIDTH/2, GAME_HEIGHT/2).createFromHTML(
        `<div class="rainbow-overlay" style="opacity:0; pointer-events:none">Game Over<br><span style="font-size:1.1em;color:#222">Press R to Restart</span><br><span class="score"></span></div>`
      ).setDepth(20).setVisible(false);

      // Input
      this.cursors = this.input.keyboard.createCursorKeys();
      this.rKey = this.input.keyboard.addKey('R');
      this.spaceKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);

      // Pointer/tap for jump (mobile friendly)
      this.input.on('pointerdown', this.handlePointerDown, this);
      this.input.on('pointerup', this.handlePointerUp, this);

      // Music
      this.bgm = this.sound.add('bgm', { volume:0.2, loop:true });
      this.bgm.play();

      // SFX
      this.jumpSfx = this.sound.add('jump', { volume:0.44 });

      // Timers for spawning
      this.nextObstacle = 0;
      this.nextStar = 0;

      // State
      this.isGameOver = false;
      this.isJumping = false;
      this.jumpTimer = 0;
      this.lastTime = this.time.now;

      // Resize
      this.scale.on('resize', this.resize, this);
      this.resize({width:window.innerWidth, height:window.innerHeight}); // initial

      // Focus
      window.focus();
    }

    update(time, delta) {
      if (this.isGameOver) {
        if (Phaser.Input.Keyboard.JustDown(this.rKey)) this.restart();
        return;
      }

      // Camera scroll speed control
      if (this.cursors.right.isDown) this.scrollSpeed = Math.min(this.scrollSpeed+3, SCROLL_MAX);
      else if (this.cursors.left.isDown) this.scrollSpeed = Math.max(this.scrollSpeed-3, SCROLL_MIN);

      // Camera moves right, scene scrolls left
      this.cameras.main.scrollX += this.scrollSpeed * (delta/1000);

      // Move ground
      this.ground.tilePositionX = this.cameras.main.scrollX * 0.9;

      // Score & distance
      this.distance += this.scrollSpeed * (delta/1000);
      this.scoreText.setText('Score: ' + this.score);

      // Unicorn jump
      let onGround = this.unicorn.body.touching.down || this.unicorn.y >= UNICORN_Y;
      if (onGround) {
        this.unicorn.y = UNICORN_Y;
        this.unicorn.body.velocity.y = 0;
        if ((Phaser.Input.Keyboard.JustDown(this.cursors.up) || Phaser.Input.Keyboard.JustDown(this.spaceKey) || this.isJumping)) {
          this.jump();
        }
      } else if (this.isJumping && this.jumpTimer < 180) {
        this.unicorn.body.velocity.y = -JUMP_VELOCITY - (this.jumpTimer/170)*JUMP_HOLD_BOOST;
        this.jumpTimer += delta;
      }

      // Unicorn stays at fixed X (camera moves)
      this.unicorn.x = this.cameras.main.scrollX + UNICORN_X;

      // Spawn obstacles
      if (time > this.nextObstacle) {
        this.spawnObstacle();
        this.nextObstacle = time + Phaser.Math.Between(OBSTACLE_MIN, OBSTACLE_MAX);
      }
      // Spawn stars
      if (time > this.nextStar) {
        this.spawnStar();
        this.nextStar = time + Phaser.Math.Between(STAR_MIN, STAR_MAX);
      }

      // Remove offscreen obstacles/stars
      this.obstacles.getChildren().forEach(o => {
        if (o.x + o.width < this.cameras.main.scrollX - 100) o.destroy();
      });
      this.stars.getChildren().forEach(s => {
        if (s.x + 40 < this.cameras.main.scrollX - 100) s.destroy();
      });
    }

    jump() {
      if (this.isGameOver) return;
      if (this.unicorn.body.velocity.y > -20 && (this.unicorn.body.touching.down || this.unicorn.y >= UNICORN_Y)) {
        this.unicorn.body.velocity.y = -JUMP_VELOCITY;
        this.jumpSfx.play();
        this.isJumping = true;
        this.jumpTimer = 0;
      }
    }
    handlePointerDown(pointer) {
      if (this.isGameOver) return;
      this.isJumping = true;
      if (this.unicorn.body.touching.down || this.unicorn.y >= UNICORN_Y) this.jump();
    }
    handlePointerUp(pointer) {
      this.isJumping = false;
      this.jumpTimer = 999;
    }

    collectStar(unicorn, star) {
      star.destroy();
      this.score += STAR_SCORE;
    }

    spawnObstacle() {
      const x = this.cameras.main.scrollX + GAME_WIDTH + 80;
      const y = GAME_HEIGHT - GROUND_HEIGHT - Phaser.Math.Between(26, 64);
      const o = this.obstacles.create(x, y, 'obstacle').setOrigin(0.5,1).setImmovable(true);
      o.body.setAllowGravity(false);
      o.body.setVelocityX(-this.scrollSpeed);
      o.width = 96;
    }
    spawnStar() {
      const x = this.cameras.main.scrollX + GAME_WIDTH + 60;
      const y = GAME_HEIGHT - GROUND_HEIGHT - Phaser.Math.Between(110, 210);
      const s = this.stars.create(x, y, 'star').setOrigin(0.5,0.5);
      s.body.setAllowGravity(false);
      s.body.setCircle(20, 4, 4);
      s.body.setVelocityX(-this.scrollSpeed * 1.05);
    }

    gameOver() {
      if (this.isGameOver) return;
      this.isGameOver = true;
      this.physics.pause();
      this.unicorn.anims.stop();
      this.bgm.stop();
      // Show overlay
      this.overlay.setVisible(true);
      const scoreHTML = `<br>Score: <span style="color:#ff5b98">${this.score}</span><br>Distance: <span style="color:#20b8ff">${Math.floor(this.distance/10)} m</span><br>Best: <span style="color:#26c93f">${Math.max(this.maxDistance, this.distance/10|0)} m</span>`;
      this.overlay.node.children[0].querySelector('.score').innerHTML = scoreHTML;
      this.overlay.node.children[0].style.opacity = 1;
      this.overlay.node.children[0].style.pointerEvents = 'auto';
      // Save best
      if ((this.distance/10|0) > this.maxDistance) {
        this.maxDistance = this.distance/10|0;
        localStorage.setItem('unicorn_best', this.maxDistance);
      }
    }

    restart() {
      this.scene.restart();
    }

    // ----- DRAWING/GRAPHICS -----
    createUnicornFrames() {
      for (let f=0;f<2;f++) {
        const g = this.make.graphics({x:0,y:0,add:false});
        // BODY
        g.fillStyle(0xffffff);
        g.fillRoundedRect(7,32,38,33,16); // body
        g.fillEllipse(27,27,30,28); // chest
        g.fillCircle(43,42,17); // hind
        // LEGS
        g.save(); g.translate(13,62);
        g.rotate((f===0)?-0.12:0.12);
        g.fillStyle(0xf0e7e7); g.fillRect(-4,0,8,22); // FL
        g.restore();
        g.save(); g.translate(25,64);
        g.rotate((f===1)?-0.24:0.06);
        g.fillStyle(0xf0e7e7); g.fillRect(-3,0,7,24); // HL
        g.restore();
        g.save(); g.translate(37,61);
        g.rotate((f===0)?0.19:-0.1);
        g.fillStyle(0xf0e7e7); g.fillRect(-3,0,6,19); // FR
        g.restore();
        g.save(); g.translate(47,65);
        g.rotate((f===1)?0.13:0);
        g.fillStyle(0xf0e7e7); g.fillRect(-2,0,6,22); // HR
        g.restore();
        // HEAD
        g.fillStyle(0xffffff); g.fillEllipse(13,15,22,18);
        // EAR
        g.fillStyle(0xffc2fb); g.fillTriangle(7,7, 12,1, 17,9);
        // HORN
        g.lineStyle(3,0xffe37a); g.beginPath(); g.moveTo(12,3); g.lineTo(12, -9); g.strokePath();
        // EYE
        g.fillStyle(0x2c2020); g.fillCircle(17,16,2.2);
        // MANE
        let maneColors = [0x8f3aff,0xd06cff,0xf7bfff];
        for(let i=0;i<3;i++) {
          g.fillStyle(maneColors[i]);
          g.fillEllipse(11 + 5*i, 21-1.5*i, 8, 8);
        }
        // TAIL
        for(let i=0;i<3;i++) {
          g.fillStyle(maneColors[2-i]);
          g.fillEllipse(47+2*i,49+5*i,8,8);
        }
        // NOSTRIL
        g.fillStyle(0x8666be); g.fillEllipse(20,19,1.4,0.9);
        // Save as texture
        g.generateTexture('unicorn'+f, 60, 86);
        g.destroy();
      }
    }
    createObstacleFrame() {
      const g = this.make.graphics({x:0,y:0,add:false});
      // Pink cloud (pillars)
      g.fillStyle(0xff7ccf);
      g.fillEllipse(40,40,78,44);
      g.fillEllipse(32,62,50,34);
      g.fillEllipse(65,60,42,25);
      g.fillStyle(0xfed8f9,0.78);
      g.fillEllipse(44,56,38,12);
      g.generateTexture('obstacle', 96, 80);
      g.destroy();
    }
    createStarFrame() {
      const g = this.make.graphics({x:0,y:0,add:false});
      // Rainbow 5-pointed star
      const cx=24, cy=24, r=20, r2=9;
      let colors = [0xff3c3c,0xffb900,0x59f140,0x00e7ff,0x9a6cff];
      for(let j=0;j<5;j++) {
        g.fillStyle(colors[j]);
        g.beginPath();
        for(let i=0;i<5;i++) {
          let ang = Phaser.Math.DegToRad(72*i-90+5*j);
          let rad = (i%2===0)?r:r2;
          if (i===0) g.moveTo(cx + Math.cos(ang)*rad, cy + Math.sin(ang)*rad);
          else g.lineTo(cx + Math.cos(ang)*rad, cy + Math.sin(ang)*rad);
        }
        g.closePath(); g.fillPath();
      }
      g.lineStyle(2,0xffffff); g.strokeCircle(cx,cy,19);
      g.generateTexture('star', 48, 48);
      g.destroy();
    }
    drawRainbowGround(tile) {
      const g = this.make.graphics({x:0,y:0,add:false});
      let stripes = [0xff3c3c,0xffb900,0x59f140,0x00e7ff,0x9a6cff,0xf767ff];
      for(let i=0;i<stripes.length;i++) {
        g.fillStyle(stripes[i]);
        g.fillRect(0,i*GROUND_HEIGHT/stripes.length, tile.width, GROUND_HEIGHT/stripes.length);
      }
      // Candy dots
      for(let i=0;i<30;i++) {
        g.fillStyle(0xffffff,0.13);
        g.fillCircle(Phaser.Math.Between(0,tile.width), Phaser.Math.Between(0,GROUND_HEIGHT), Phaser.Math.Between(4,12));
      }
      g.generateTexture('ground', tile.width, GROUND_HEIGHT);
      tile.setTexture('ground');
      g.destroy();
    }

    // --- Responsive scaling ---
    resize(gameSize) {
      let w = window.innerWidth, h = window.innerHeight;
      let ratio = GAME_WIDTH/GAME_HEIGHT;
      let scale = Math.min(w/GAME_WIDTH, h/GAME_HEIGHT);
      this.scale.resize(GAME_WIDTH, GAME_HEIGHT);
      this.game.canvas.style.width = (GAME_WIDTH*scale) + 'px';
      this.game.canvas.style.height = (GAME_HEIGHT*scale) + 'px';
      this.game.canvas.style.margin = 'auto';
      this.game.canvas.style.position = 'absolute';
      this.game.canvas.style.left = ((w - GAME_WIDTH*scale)/2) + 'px';
      this.game.canvas.style.top = ((h - GAME_HEIGHT*scale)/2) + 'px';
    }
  }

  // Phaser config
  const config = {
    type: Phaser.AUTO,
    width: GAME_WIDTH,
    height: GAME_HEIGHT,
    backgroundColor: null,
    parent: null,
    scene: GameScene,
    scale: { mode: Phaser.Scale.NONE },
    physics: {
      default: 'arcade',
      arcade: {
        gravity: { y: 0 },
        debug: false
      }
    },
    dom: { createContainer:true }
  };

  window.addEventListener('resize', () => {
    if(window.game && window.game.scene) {
      const scene = window.game.scene.getAt(0);
      if (scene && scene.resize) scene.resize();
    }
  });

  window.game = new Phaser.Game(config);
})();