// Frontend Controller for Beat The Rocky
// Connects to ASP.NET Core REST API

(function () {
    const API_BASE = '/api/game';

    // State
    let sessionId = null;
    let gameState = null;
    let isProcessing = false;

    // Scores
    let scores = {
        rockyWins: 0,
        draws: 0,
        playerWins: 0
    };

    // DOM Elements
    const rockySpeech = document.getElementById('rockySpeech');
    const turnIndicator = document.getElementById('turnIndicator');
    const turnText = document.getElementById('turnText');
    const rockyWinsScore = document.getElementById('rockyWinsScore');
    const drawsScore = document.getElementById('drawsScore');
    const playerWinsScore = document.getElementById('playerWinsScore');

    const tossArena = document.getElementById('tossArena');
    const boardArena = document.getElementById('boardArena');
    const coin = document.getElementById('coin');
    const btnHeads = document.getElementById('btnHeads');
    const btnTails = document.getElementById('btnTails');
    const cells = document.querySelectorAll('.cell');
    const rematchBtn = document.getElementById('rematchBtn');
    const resetScoreBtn = document.getElementById('resetScoreBtn');
    const soundToggleBtn = document.getElementById('soundToggleBtn');
    const soundIcon = document.getElementById('soundIcon');
    const soundText = document.getElementById('soundText');

    // Load persisted scores
    function loadScores() {
        const saved = localStorage.getItem('beatTheRocky_scores');
        if (saved) {
            try {
                scores = JSON.parse(saved);
            } catch (e) {
                console.error('Error parsing scores', e);
            }
        }
        updateScoreboardDisplay();
    }

    function saveScores() {
        localStorage.setItem('beatTheRocky_scores', JSON.stringify(scores));
        updateScoreboardDisplay();
    }

    function updateScoreboardDisplay() {
        rockyWinsScore.textContent = scores.rockyWins;
        drawsScore.textContent = scores.draws;
        playerWinsScore.textContent = scores.playerWins;
    }

    // Audio toggle
    soundToggleBtn.addEventListener('click', () => {
        const isEnabled = window.soundFx.toggleSound();
        soundIcon.textContent = isEnabled ? '🔊' : '🔇';
        soundText.textContent = isEnabled ? 'Sound ON' : 'Sound OFF';
    });

    // Start New Game Session
    async function startNewSession() {
        try {
            isProcessing = true;
            const res = await fetch(`${API_BASE}/new`, { method: 'POST' });
            if (!res.ok) throw new Error('Failed to create session');
            gameState = await res.json();
            sessionId = gameState.id;
            resetUIForToss();
        } catch (err) {
            console.error('Error initializing game:', err);
            rockySpeech.textContent = "Error connecting to Rocky's corner. Is the backend server running?";
        } finally {
            isProcessing = false;
        }
    }

    function resetUIForToss() {
        tossArena.classList.remove('hidden');
        boardArena.classList.add('hidden');
        btnHeads.disabled = false;
        btnTails.disabled = false;
        coin.className = 'coin';

        cells.forEach(cell => {
            cell.textContent = '';
            cell.className = 'cell';
        });

        turnText.textContent = 'Toss the coin to start the match';
        rockySpeech.textContent = gameState?.rockyComment || "Toss the coin to see who gets the opening punch!";
    }

    // Handle Coin Toss
    async function handleCoinToss(choice) {
        if (isProcessing || !sessionId) return;
        isProcessing = true;
        btnHeads.disabled = true;
        btnTails.disabled = true;

        window.soundFx.playCoinFlip();
        rockySpeech.textContent = `Coin is flying! You bet on ${choice}...`;

        try {
            const res = await fetch(`${API_BASE}/${sessionId}/toss`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ choice: choice })
            });

            if (!res.ok) throw new Error('Coin toss request failed');
            const data = await res.json();
            gameState = data;

            // Trigger visual 3D coin spin
            coin.className = 'coin';
            void coin.offsetWidth; // Trigger reflow
            const spinClass = data.tossResult === 'Heads' ? 'flipping-heads' : 'flipping-tails';
            coin.classList.add(spinClass);

            // Wait for coin animation to land (1.6s)
            setTimeout(() => {
                window.soundFx.playCoinLand();

                setTimeout(() => {
                    tossArena.classList.add('hidden');
                    boardArena.classList.remove('hidden');
                    window.soundFx.playBell();
                    renderBoard();
                    isProcessing = false;
                }, 600);

            }, 1600);

        } catch (err) {
            console.error('Toss error:', err);
            rockySpeech.textContent = "Referee timeout! Couldn't flip coin.";
            btnHeads.disabled = false;
            btnTails.disabled = false;
            isProcessing = false;
        }
    }

    // Render Board and Game State
    function renderBoard() {
        if (!gameState) return;

        // Render each cell
        gameState.board.forEach((val, idx) => {
            const cell = cells[idx];
            cell.textContent = val;
            cell.classList.remove('symbol-x', 'symbol-o', 'taken');

            if (val === 'X') {
                cell.classList.add('symbol-x', 'taken');
            } else if (val === 'O') {
                cell.classList.add('symbol-o', 'taken');
            }
        });

        // Highlight Winning Combo if any
        if (gameState.winningCombination && gameState.winningCombination.length > 0) {
            gameState.winningCombination.forEach(idx => {
                cells[idx].classList.add('winning-cell');
            });
        }

        // Update Rocky Commentary
        rockySpeech.textContent = gameState.rockyComment;

        // Update Turn Indicator
        if (gameState.status === 'InProgress') {
            const isMyTurn = gameState.currentTurn === gameState.humanSymbol;
            turnText.textContent = isMyTurn ? `Your Turn (${gameState.humanSymbol})` : `Rocky is thinking... (${gameState.rockySymbol})`;
        } else if (gameState.status === 'RockyWon') {
            turnText.textContent = '🥊 Rocky Won by Knockout!';
        } else if (gameState.status === 'Draw') {
            turnText.textContent = '🛡️ Round Ended in a Draw!';
        } else if (gameState.status === 'PlayerWon') {
            turnText.textContent = '🎉 You beat Rocky?! (Inconceivable!)';
        }
    }

    // Handle Player Cell Move
    async function handleCellClick(e) {
        const cell = e.currentTarget;
        const index = parseInt(cell.getAttribute('data-index'), 10);

        if (isProcessing || !gameState || gameState.isGameOver) return;
        if (gameState.currentTurn !== gameState.humanSymbol) return;
        if (gameState.board[index] !== '') return;

        isProcessing = true;

        // Optimistic UI display for responsiveness
        cell.textContent = gameState.humanSymbol;
        cell.classList.add(gameState.humanSymbol === 'X' ? 'symbol-x' : 'symbol-o', 'taken');
        window.soundFx.playPunch(false);

        try {
            const res = await fetch(`${API_BASE}/${sessionId}/move`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ cellIndex: index })
            });

            if (!res.ok) {
                const errData = await res.json();
                throw new Error(errData.error || 'Move failed');
            }

            const data = await res.json();
            gameState = data;

            // Small delay for Rocky's punch sound for dramatic effect
            setTimeout(() => {
                renderBoard();

                if (gameState.status === 'RockyWon') {
                    window.soundFx.playKnockout();
                    scores.rockyWins++;
                    saveScores();
                } else if (gameState.status === 'Draw') {
                    window.soundFx.playBell();
                    scores.draws++;
                    saveScores();
                } else if (gameState.status === 'PlayerWon') {
                    scores.playerWins++;
                    saveScores();
                } else {
                    window.soundFx.playPunch(true);
                }

                isProcessing = false;
            }, 250);

        } catch (err) {
            console.error('Move error:', err);
            rockySpeech.textContent = "Illegal move attempted or connection dropped.";
            renderBoard();
            isProcessing = false;
        }
    }

    // Rematch & Reset
    async function handleRematch() {
        if (!sessionId) {
            startNewSession();
            return;
        }

        try {
            const res = await fetch(`${API_BASE}/${sessionId}/reset`, { method: 'POST' });
            if (!res.ok) throw new Error('Reset failed');
            gameState = await res.json();
            resetUIForToss();
        } catch (err) {
            startNewSession();
        }
    }

    function handleResetScore() {
        if (confirm('Are you sure you want to reset the scoreboard?')) {
            scores = { rockyWins: 0, draws: 0, playerWins: 0 };
            saveScores();
        }
    }

    // Event Listeners
    btnHeads.addEventListener('click', () => handleCoinToss('Heads'));
    btnTails.addEventListener('click', () => handleCoinToss('Tails'));
    cells.forEach(cell => cell.addEventListener('click', handleCellClick));
    rematchBtn.addEventListener('click', handleRematch);
    resetScoreBtn.addEventListener('click', handleResetScore);

    // Initial boot
    loadScores();
    startNewSession();
})();
