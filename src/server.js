const http = require('http');

const store = {
  games: new Map(),
  chats: new Map(),
};

function createId(prefix) {
  return prefix + '-' + Math.random().toString(36).substring(2, 8);
}

function parseBody(req) {
  return new Promise((resolve) => {
    let data = '';
    req.on('data', chunk => data += chunk);
    req.on('end', () => {
      try {
        resolve(data ? JSON.parse(data) : {});
      } catch {
        resolve({});
      }
    });
  });
}

function send(res, status, body) {
  const data = body ? JSON.stringify(body) : '';
  res.writeHead(status, { 'Content-Type': 'application/json' });
  res.end(data);
}

function createGame(playerName, settings) {
  const player = { id: createId('player'), name: playerName, isReady: false, isDrawer: false };
  const game = {
    id: createId('game'),
    settings,
    status: 'waiting',
    players: [player],
    currentRound: {
      roundNumber: 1,
      currentTurn: { turnNumber: 1, drawerId: player.id, status: 'not_started' }
    },
    scoreRecords: [],
  };
  store.games.set(game.id, game);
  return game;
}

function joinGame(gameId, playerName) {
  const game = store.games.get(gameId);
  if (!game) return null;
  const player = { id: createId('player'), name: playerName, isReady: false, isDrawer: false };
  game.players.push(player);
  return player;
}

function ready(gameId) {
  const game = store.games.get(gameId);
  if (!game) return;
  game.players.forEach(p => p.isReady = true);
}

function startGame(gameId) {
  const game = store.games.get(gameId);
  if (!game) return;
  game.status = 'playing';
}

function setAnswer(gameId, answer) {
  const game = store.games.get(gameId);
  if (!game) return;
  game.currentRound.currentTurn.answer = answer;
  game.currentRound.currentTurn.status = 'drawing';
}

function submitAnswer(gameId, answer) {
  const game = store.games.get(gameId);
  if (!game) return false;
  const correct = game.currentRound.currentTurn.answer === answer;
  if (correct) game.currentRound.currentTurn.status = 'finished';
  return correct;
}

function addChat(gameId, content) {
  if (!store.games.has(gameId)) return null;
  const msg = { id: createId('chat'), gameId, playerId: 'anon', playerName: 'anon', content, type: 'normal', timestamp: Date.now() };
  const list = store.chats.get(gameId) || [];
  list.push(msg);
  store.chats.set(gameId, list);
  return msg;
}

function getChats(gameId) {
  return store.chats.get(gameId) || [];
}

const server = http.createServer(async (req, res) => {
  const url = req.url || '';
  const [path] = url.split('?');
  const parts = path.split('/').filter(Boolean);

  if (req.method === 'POST' && path === '/games') {
    const body = await parseBody(req);
    const game = createGame(body.playerName, body.settings || {});
    return send(res, 201, game);
  }

  if (parts[0] === 'games') {
    const gameId = parts[1];
    if (req.method === 'POST' && parts[2] === 'join') {
      const body = await parseBody(req);
      const player = joinGame(gameId, body.playerName);
      if (!player) return send(res, 404);
      return send(res, 200, player);
    }
    if (req.method === 'POST' && parts[2] === 'ready') {
      ready(gameId);
      return send(res, 200, {});
    }
    if (req.method === 'POST' && parts[2] === 'answer') {
      const body = await parseBody(req);
      setAnswer(gameId, body.answer);
      return send(res, 200, {});
    }
    if (req.method === 'POST' && parts[2] === 'drawing') {
      await parseBody(req);
      return send(res, 200, {});
    }
    if (req.method === 'POST' && parts[2] === 'submit') {
      const body = await parseBody(req);
      const result = submitAnswer(gameId, body.answer);
      return send(res, 200, { isCorrect: result });
    }
    if (req.method === 'POST' && parts[2] === 'chat') {
      const body = await parseBody(req);
      const msg = addChat(gameId, body.content);
      if (!msg) return send(res, 404);
      return send(res, 200, msg);
    }
    if (req.method === 'GET' && parts.length === 2) {
      const game = store.games.get(gameId);
      if (!game) return send(res, 404);
      return send(res, 200, game);
    }
    if (req.method === 'GET' && parts[2] === 'chat') {
      const list = getChats(gameId);
      return send(res, 200, list);
    }
    if (req.method === 'POST' && parts[2] === 'start') {
      startGame(gameId);
      return send(res, 200, {});
    }
  }

  send(res, 404);
});

if (require.main === module) {
  const port = 3000;
  server.listen(port, () => {
    console.log('Server running on port', port);
  });
}

module.exports = { server, store };
