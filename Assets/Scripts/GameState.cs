using System;
using System.Collections.Generic;
using System.Linq;

/* Pretty Board
  0--------1--------2
  |        |        |
  |  3-----4-----5  |
  |  |     |     |  |
  |  |  6--7--8  |  |
  9--10-11    12-13-14
  |  |  15-16-17 |  |
  |  |     |     |  |
  |  18----19----20 |
  |        |        |
  21-------22-------23
*/

/* Ugly board
	0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22
*/

public class GameState
{
	// the total number of playable positions
	public static int NUMBER_OF_POSITIONS = 24;
	// each players starting number of stones
	public static int PLAYER_NUMBER_OF_STONES = 9;
	// how many moves without mills before a game is declared a draw
	public static int NUMBER_OF_MOVES_FOR_DRAW = 50;

	private int _remainingStonesToPlace = 2 * PLAYER_NUMBER_OF_STONES;
	public int remainingStonesToPlace
	{
		get
		{
			return _remainingStonesToPlace;
		}

		private set
		{

			_remainingStonesToPlace = value;
		}
	}

	private int[] board;
	private int movesWithoutMill = 0;
	public int currentPlayer { get; private set; }
	private int currentPlayerIndex { get { return this.currentPlayer - 1; } }
	public int opponentPlayer { get { return 3 - this.currentPlayer; } }
	private int opponentPlayerIndex { get { return this.opponentPlayer - 1; } }
	private int[] playersAvailableStones = new int[] { PLAYER_NUMBER_OF_STONES, PLAYER_NUMBER_OF_STONES };

	// e.g. <9; [10,11], [0,21]> this means that the index 9 can create a mill in combinations 
	// with stones on 10 and 11 or 0 and 21 so we just need to check those other two indexes 
	// to check for the creation of a mill
	private Dictionary<int, List<int[]>> millConnections = new Dictionary<int, List<int[]>>();

	// e.g. <9; 10,0,21> this means that the index 9 is directly connected to index 10, 0 and 21
	private Dictionary<int, List<int>> boardConnections = new Dictionary<int, List<int>>();

	public delegate void CurrentPlayerChanged();
	public event CurrentPlayerChanged OnPlayerChange;
	public delegate void GamePhaseChanged();
	public event GamePhaseChanged OnGamePhaseChanged;

	public GameState(int startingPlayer = 1)
	{
		this.currentPlayer = startingPlayer;

		InitializeBoard();
		InitializeMillConnections();
		InitializeBoardConnections();
	}

	public GameState Clone()
	{
		GameState gs = new GameState();
		gs._remainingStonesToPlace = _remainingStonesToPlace;
		gs.currentPlayer = currentPlayer;
		gs.playersAvailableStones = new int[] {
			playersAvailableStones[0],
			playersAvailableStones[1]
		};
		gs.movesWithoutMill = movesWithoutMill;
		gs.board = (int[])board.Clone();

		return gs;
	}

	/// <summary>
	/// Initializes the board with zeroes in every position.
	/// </summary>
	private void InitializeBoard()
	{
		board = new int[NUMBER_OF_POSITIONS];

		foreach (int boardPosition in board)
		{
			board[boardPosition] = 0;
		}
	}

	public int[] GetBoard()
	{
		return (int[])board.Clone();
	}

	/// <summary>
	/// Initializes the mill connections.
	/// </summary>
	private void InitializeMillConnections()
	{
		// TODO Think about doing this using board simmetries
		millConnections.Add(0, new List<int[]>() {
				new int[] { 1, 2 },
				new int[] { 9, 21 }
			});

		millConnections.Add(1, new List<int[]>() {
				new int[] { 0, 2 },
				new int[] { 4, 7 }
			});

		millConnections.Add(2, new List<int[]>() {
				new int[] { 0, 1 },
				new int[] { 14, 23 }
			});

		millConnections.Add(3, new List<int[]>() {
				new int[] { 4, 5 },
				new int[] { 10, 18 }
			});

		millConnections.Add(4, new List<int[]>() {
				new int[] { 1, 7 },
				new int[] { 3, 5 }
			});

		millConnections.Add(5, new List<int[]>() {
				new int[] { 3, 4 },
				new int[] { 13, 20 }
			});

		millConnections.Add(6, new List<int[]>() {
				new int[] { 7, 8 },
				new int[] { 11, 15 }
			});

		millConnections.Add(7, new List<int[]>() {
				new int[] { 6, 8 },
				new int[] { 1, 4 }
			});

		millConnections.Add(8, new List<int[]>() {
				new int[] { 6, 7 },
				new int[] { 12, 17 }
			});

		millConnections.Add(9, new List<int[]>() {
				new int[] { 0, 21 },
				new int[] { 10, 11 }
			});

		millConnections.Add(10, new List<int[]>() {
				new int[] { 3, 18 },
				new int[] { 9, 11 }
			});

		millConnections.Add(11, new List<int[]>() {
				new int[] { 6, 15 },
				new int[] { 9, 10 }
			});

		millConnections.Add(12, new List<int[]>() {
				new int[] { 8, 17 },
				new int[] { 13, 14 }
			});

		millConnections.Add(13, new List<int[]>() {
				new int[] { 5, 20 },
				new int[] { 12, 14 }
			});

		millConnections.Add(14, new List<int[]>() {
				new int[] { 2, 23 },
				new int[] { 12, 13 }
			});

		millConnections.Add(15, new List<int[]>() {
				new int[] { 6, 11 },
				new int[] { 16, 17 }
			});

		millConnections.Add(16, new List<int[]>() {
				new int[] { 15, 17 },
				new int[] { 19, 22 }
			});

		millConnections.Add(17, new List<int[]>() {
				new int[] { 15, 16 },
				new int[] { 8, 12 }
			});

		millConnections.Add(18, new List<int[]>() {
				new int[] { 3, 10 },
				new int[] { 19, 20 }
			});

		millConnections.Add(19, new List<int[]>() {
				new int[] { 18, 20 },
				new int[] { 16, 22 }
			});

		millConnections.Add(20, new List<int[]>() {
				new int[] { 5, 13 },
				new int[] { 18, 19 }
			});

		millConnections.Add(21, new List<int[]>() {
				new int[] { 0, 9 },
				new int[] { 22, 23 }
			});

		millConnections.Add(22, new List<int[]>() {
				new int[] { 21, 23 },
				new int[] { 16, 19 }
			});

		millConnections.Add(23, new List<int[]>() {
				new int[] { 21, 22 },
				new int[] { 2, 14 }
			});
	}

	/// <summary>
	/// Initializes the board connections.
	/// </summary>
	private void InitializeBoardConnections()
	{
		// note that angles have 2 connections, edges have 3 while the "crosses" have 4 
		/* Pretty Board
		 * 0--------1--------2
		 * |        |        |
		 * |  3-----4-----5  |
		 * |  |     |     |  |
		 * |  |  6--7--8  |  |
		 * 9--10-11    12-13-14
		 * |  |  15-16-17 |  |
		 * |  |     |     |  |
		 * |  18----19----20 |
		 * |        |        |
		 * 21-------22-------23
		*/
		boardConnections.Add(0, new List<int>() { 1, 9 });
		boardConnections.Add(1, new List<int>() { 0, 2, 4 });
		boardConnections.Add(2, new List<int>() { 1, 14 });
		boardConnections.Add(3, new List<int>() { 4, 10 });
		boardConnections.Add(4, new List<int>() { 1, 3, 5, 7 });
		boardConnections.Add(5, new List<int>() { 4, 13 });
		boardConnections.Add(6, new List<int>() { 7, 11 });
		boardConnections.Add(7, new List<int>() { 4, 6, 8 });
		boardConnections.Add(8, new List<int>() { 7, 12 });
		boardConnections.Add(9, new List<int>() { 0, 10, 21 });
		boardConnections.Add(10, new List<int>() { 3, 9, 11, 18 });
		boardConnections.Add(11, new List<int>() { 6, 10, 15 });
		boardConnections.Add(12, new List<int>() { 8, 13, 17 });
		boardConnections.Add(13, new List<int>() { 5, 12, 14, 20 });
		boardConnections.Add(14, new List<int>() { 2, 13, 23 });
		boardConnections.Add(15, new List<int>() { 11, 16 });
		boardConnections.Add(16, new List<int>() { 15, 17, 19 });
		boardConnections.Add(17, new List<int>() { 12, 16 });
		boardConnections.Add(18, new List<int>() { 10, 19 });
		boardConnections.Add(19, new List<int>() { 16, 18, 20, 22 });
		boardConnections.Add(20, new List<int>() { 13, 19 });
		boardConnections.Add(21, new List<int>() { 9, 22 });
		boardConnections.Add(22, new List<int>() { 19, 21, 23 });
		boardConnections.Add(23, new List<int>() { 22, 14 });
	}

	public void DoMove(GameMove move)
	{
        if(move.from == -1 && move.to == -1 && move.remove == -1) {
            throw new ArgumentException("Illegal move, all indexes are -1", "move");
        }
        
        if(move.from > board.Length ||
            move.to > board.Length ||
            move.remove > board.Length ||
            move.from < -1 ||
            move.to < -1 ||
            move.remove < -1) {
            throw new ArgumentException("Board positions are out of range", "move");
        }

        if (move.to != -1 && move.from == -1 && move.remove == -1 && board[move.to] != 0)
		{
			throw new Exception(string.Format("Illegal move! Destination {0} already taken.", move.to));
		}

        if (move.from != -1 && move.to != -1 && board[move.from] != currentPlayer)
		{
			throw new Exception(string.Format("Illegal move! Player {0} doesn't have a token in position {1}", this.currentPlayer, move.from));
		}

        if (move.from != -1 && move.to != -1 && board[move.to] != 0)
        {
            throw new Exception(string.Format("Illegal move from {0} to {1}. Destination is not empty.", move.from, move.to));
        }

        /*
        if (move.from != -1 && move.to != -1 && !boardConnections[move.from].Contains(move.to))
		{
			throw new Exception(string.Format("Illegal move from {0} to {1}. Positions are not adjacent.", move.from, move.to));
		}
        */

		if (move.remove != -1 && !MakesAMill(move.to))
		{
			throw new Exception("Illegal move! Trying to remove without a move having made a mill");
		}

		if (move.remove != -1 && board[move.remove] != this.opponentPlayer)
		{
            throw new Exception(string.Format("Illegal move! Can't remove from index {0}. Player {1} doesn't have a stone in position {0}", move.remove, opponentPlayer));
		}

		if (move.remove != -1 && !GetRemovableIndexes(opponentPlayer).Contains(move.remove))
		{
			throw new Exception("Illegal move! Trying to remove opponent token which is part of a mill");
		}

		// A move that just places a stone
		if (move.from == -1 && move.to != -1 && move.remove == -1)
		{
			board[move.to] = this.currentPlayer;

			remainingStonesToPlace--;

			movesWithoutMill++;
		}

		// A move that by placing a stone creats a mill 
		// and thus remove an opponent stone 
		if (move.from == -1 && move.to != -1 && move.remove != -1)
		{
			board[move.to] = this.currentPlayer;
			board[move.remove] = 0;

			playersAvailableStones[opponentPlayerIndex]--;
			remainingStonesToPlace--;

			movesWithoutMill = 0;
		}

		// A move that just moves a stone from one position to another
		if (move.from != -1 && move.to != -1 && move.remove == -1)
		{
			board[move.from] = 0;
			board[move.to] = this.currentPlayer;

			movesWithoutMill++;
		}

		// A move that by moving a stone from one position to another creates a mill
		// and thus remove an opponent stone
		if (move.from != -1 && move.to != -1 && move.remove != -1)
		{
			board[move.from] = 0;
			board[move.to] = this.currentPlayer;
			board[move.remove] = 0;

			playersAvailableStones[opponentPlayerIndex]--;

			movesWithoutMill = 0;
		}

		this.currentPlayer = opponentPlayer;

		if (move.from == -1 && remainingStonesToPlace == 0)
		{
			if (OnGamePhaseChanged != null)
			{
				OnGamePhaseChanged();
			}
		}

		if (OnPlayerChange != null)
		{
			OnPlayerChange();
		}
	}

	/// <summary>
	/// Gets all the possible moves including possible indexes to remove.
	/// </summary>
	public List<GameMove> GetMoves()
	{
		List<GameMove> moves = new List<GameMove>();

		if (remainingStonesToPlace > 0)
		{
			// Placing phase
			for (int boardIndex = 0; boardIndex < NUMBER_OF_POSITIONS; boardIndex++)
			{
				if (board[boardIndex] == 0)
				{
					AddMove(ref moves, -1, boardIndex);
				}
			}
		}
		else
		{
			if (playersAvailableStones[this.currentPlayer - 1] == 3)
			{
				// Flying phase
				for (int boardIndex = 0; boardIndex < NUMBER_OF_POSITIONS; boardIndex++)
				{
					if (board[boardIndex] == this.currentPlayer)
					{
						for (int flyToIndex = 0; flyToIndex < NUMBER_OF_POSITIONS; flyToIndex++)
						{
							if (board[flyToIndex] == 0)
							{
								AddMove(ref moves, boardIndex, flyToIndex);
							}
						}
					}
				}
			}
			else
			{
				// Moving phase
				for (int boardIndex = 0; boardIndex < NUMBER_OF_POSITIONS; boardIndex++)
				{
					if (board[boardIndex] == this.currentPlayer)
					{
						foreach (int connectedIndex in boardConnections[boardIndex])
						{
							if (board[connectedIndex] == 0)
							{
								AddMove(ref moves, boardIndex, connectedIndex);
							}
						}
					}
				}
			}
		}

		return moves;
	}

	/// <summary>
	/// Adds the move to the list of moves.
	/// If a specific move creates a mill, remove indexes are populated as well.
	/// </summary>
	private void AddMove(ref List<GameMove> moves, int from, int to)
	{
		if (MakesAMill(to))
		{
			foreach (int removeIndex in GetRemovableIndexes())
			{
				moves.Add(new GameMove(from, to, removeIndex));
			}
		}
		else
		{
			moves.Add(new GameMove(from, to, -1));
		}
	}

	/// <summary>
	/// Whether a player makes a mill by placing a stone at the given board position.
	/// </summary>
	public bool MakesAMill(int boardIndex, int player = 0)  
	{
		player = player == 0 ? this.currentPlayer : player;

		List<int[]> connectedPositions = millConnections[boardIndex];

		for (int millsPossibility = 0; millsPossibility < connectedPositions.Count; millsPossibility++)
		{
			if (board[connectedPositions[millsPossibility][0]] == player && board[connectedPositions[millsPossibility][1]] == player)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The mills created from board position by the player.
	/// </summary>
	public List<int[]> MillsFromIndex(int boardIndex, int player = 0)
	{
		player = player == 0 ? this.currentPlayer : player;

		List<int[]> connectedPositions = millConnections[boardIndex];
		List<int[]> millsFound = new List<int[]>();
		for (int millsPossibility = 0; millsPossibility < connectedPositions.Count; millsPossibility++)
		{
			if (board[connectedPositions[millsPossibility][0]] == player && board[connectedPositions[millsPossibility][1]] == player)
			{
				millsFound.Add(new int[3] {
					connectedPositions[millsPossibility][0],
					connectedPositions[millsPossibility][1],
					boardIndex });
			}
		}

		return millsFound;
	}

	/// <summary>
	/// Checks if a stone in a given board position is already part of a mill or not.
	/// </summary>
	/// <returns><c>true</c>, if board position contains a stone that is part of a Mill, <c>false</c> otherwise.</returns>
	/// <param name="boardIndex">The index of the board.</param>
	private bool IsPartOfAMill(int boardIndex)
	{
		int player = board[boardIndex];
		if (player == 0)
		{
			return false;
		}

		return MakesAMill(boardIndex, player);
	}

	/// <summary>
	/// Gets the indexes of the positions containing a stone that can be removed for a given player.
	/// </summary>
	/// <returns>The removable indexes.</returns>
	/// <param name="player">The player to get the indexes for, default is current player.</param>
	public List<int> GetRemovableIndexes(int player = 0)
	{
		// we use the opponent as default
		player = player == 0 ? opponentPlayer : player;

		List<int> removableIndexes = new List<int>();

		List<int> allIndexes = new List<int>();

		for (int boardIndex = 0; boardIndex < NUMBER_OF_POSITIONS; boardIndex++)
		{
			if (board[boardIndex] == player)
			{
				if (!IsPartOfAMill(boardIndex))
				{
					removableIndexes.Add(boardIndex);
				}
				else
				{
					allIndexes.Add(boardIndex);
				}
			}
		}

		return removableIndexes.Count > 0 ? removableIndexes : allIndexes;
	}

	/// <summary>
	/// Returns all the indexes a stone in the provided board index can move to respecting the phase of the game.
	/// Note: This is limited to the current player so the current player has to be in the provided index.
	/// </summary>
	/// <returns>The move to indexes of the board the current player can legally move to.</returns>
	/// <param name="from">All the board indexes a token in the provided index can move to.</param>
	public List<int> CanMoveToIndexes(int from)
	{
		if (board[from] != this.currentPlayer)
		{
			throw new ArgumentException("Current player is not in the provided board position", "from");
		}
		List<int> indexesCanMoveTo = this.GetMoves().Where(move => move.from == from).Select(x => x.to).ToList();
		return indexesCanMoveTo;
	}

	/// <summary>
	/// Gets the player number of mills.
	/// </summary>
	public int GetPlayerNumberOfMills(int player)
	{
		if (player == 0)
		{
			return 0;
		}

		int numberOfMillsFound = 0;

		for (int boardIndex = 0; boardIndex < NUMBER_OF_POSITIONS; boardIndex++)
		{
			if (board[boardIndex] == player && IsPartOfAMill(boardIndex))
			{
				numberOfMillsFound++;
			}
		}

		return numberOfMillsFound / 3;
	}

	/// <summary>
	/// Gets the score for the specified player. 
	/// There's not really a concept of score in Nine Men's Morris so in this case the number of tokens in play are returned.
	/// </summary>
	/// <returns>The score.</returns>
	/// <param name="player">Player.</param>
	public int GetScore(int player)
	{
		return playersAvailableStones[player - 1];
	}

	/// <summary>
	/// Whether a game is in a terminale state or not.
	/// For a game to be in a terminal state one of the two players has to be reduced to two stones
	/// or the current player cannot perform any moves and is thus blocked or there have been too many
	/// moves without any of the players creating a mill (cyclical play)
	/// </summary>
	/// <returns><c>true</c>, if the game is in a terminal state, <c>false</c> otherwise.</returns>
	public bool IsTerminal()
	{
		return playersAvailableStones[0] <= 2 || playersAvailableStones[1] <= 2 ||
			GetMoves().Count == 0 || movesWithoutMill == NUMBER_OF_MOVES_FOR_DRAW;
	}

	/// <summary>
	/// Returns the winner of the current if available.
	/// </summary>
	/// <returns>-1 id state is not terminal. 1/2 depending of the winning player if available. 0 if it is a draw. </returns>
	public int Winner()
	{
		if (!IsTerminal())
		{
			return -1;
		}

		if (playersAvailableStones[0] == 2)
		{
			return 2;
		}

		if (playersAvailableStones[1] == 2)
		{
			return 1;
		}

		if (!this.GetMoves().Any())
		{
			return opponentPlayer;
		}

		return 0;
	}
}