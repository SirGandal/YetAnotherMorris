using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System;
using System.IO;
using System.Security.Permissions;
using System.Security;
using UnityEngine.SceneManagement;
using System.Threading;

public class Main : MonoBehaviour
{
	// Player 1 is on the left of color black
	// Player 2 is on the right of color white

	public GameObject board;
	public Text text;
    public GameObject turnIndicator;
	private GameObject selectedTile;
	private int savedClickedTileIndexTo = -1;
	private int savedClickedTileIndexFrom = -1;
	private int selectedBoardIndex;
	private GameState state;
	private List<int[]> currentMills;
	private List<GameObject> deactivatedTiles = new List<GameObject>(), deactivatedTokens = new List<GameObject>();
	private UnityAction changeOfPhaseListener;
	private Sprite[] playerSprites;
	private string logsFilePath;
	private Logger logger;
	
	private bool changingTurn = false;
	private Color avatarWinnerColor = Color.green;
	private Color avatarPlayingColor = new Color32(255, 161, 19, 255);
	private Color avatarNotPlayingColor = Color.black;
	private Color tokenNotClickable = new Color32(255, 255, 255, 126);
	private Color tokenIndicatorUsed = new Color32(255, 255, 255, 75);
	private Color tokenClickable = new Color32(255, 255, 255, 255);
	DebugLogWriter debugLogProgressWriter = new DebugLogWriter();
	public GameObject verticalLayout;
	public bool useHorizontal = false;
	
    #region player 1 variables
    public GameObject p1;
	public GameObject player1Token;
    public Text player1Score;
    public Text player1Name;
	public Image player1AvatarBackground;
	public Sprite player1TokenNoHighlight;
	public Sprite player1TokenHighlight;
	public Sprite player1TokenMove;
	private List<GameObject> player1TokenLeftIndicators;
	private List<GameObject> player1TokensToPlace;
	#endregion

	#region player 2 variables
    public GameObject p2;
	public GameObject player2Token;
    public Text player2Score;
    public Text player2Name;
	public Image player2AvatarBackground;
	public Sprite player2TokenNoHighlight;
	public Sprite player2TokenHighlight;
	public Sprite player2TokenMove;
	private List<GameObject> player2TokenLeftIndicators;
	private List<GameObject> player2TokensToPlace;
	#endregion

	private static String[] randomNames = { "Albert Einstein",
		"Stephen Hawking", "Sheldon Cooper", "Dr.House", "Michael Jackson",
		"Michael Bay", "Mark Zuckerberg", "Alfred Hitchcock", "Amy Whinehouse",
		"Angelina Jolie", "Arnold Schwarzenegger", "Barak Obama", "Batman",
		"David Beckham", "Bruce Willis", "Charlie Chaplin", "Clint Eastwood",
		"Conan O' Brien", "Condoleezza Rice", "Charles Darwin", "Dexter Morgan",
		"Frodo", "Sauron", "George W Bush", "Hannibal", "Harrison Ford", "Harry Potter",
		"John Locke", "Johnny Depp", "John Wayne", "Karl Marx", "Larry King", "Leonardo Dicaprio",
		"Manny Pacquiao", "Marilyn Manson", "Matt Damon", "Meryl Streep", "Mr Bean",
		"Paris Hilton", "Prince Charles", "Quentin Tarantino", "Robert Pattinson",
		"Samuel L. Jackson", "Simon Cowell", "Snoop Lion", "Spielberg", "Steven Seagal",
		"Terminator", "Tom Cruise", "Will Smith", "Nelson Mandela", "Iron Man", "Hulk", "Thor",
		"Loki", "Captain America", "Black Widow", "Phil Coulson"};

	// Use this for initialization
	void Start()
	{
		Console.SetOut(debugLogProgressWriter);

		EventManager.StartListening("GameStarted", new UnityAction(StartGame));

		try
		{
			logsFilePath = String.Format("{0}/logs/{1:dd-MM-yyyy_hh-mm-ss-tt}.txt", Directory.GetCurrentDirectory(), DateTime.Now);
			(new FileInfo(logsFilePath)).Directory.Create();
			logger = new Logger(logsFilePath);
		}
		catch (Exception)
		{
			logger = new Logger(string.Empty);
			logger.LogReadable(string.Format("=!=\tDon't have permission to write to {0}", logsFilePath));
		}

		//TestMove();

		text.text = string.Empty;
		player1Name.text = string.Empty;
		player2Name.text = string.Empty;
		SetUpGame();

		//UpdatePlayerTurnIndicator();
		GatherTokenLeftIndicators();
		GatherTokensToPlace();

		//changeOfPhaseListener = new UnityAction (GamePhaseChanged);
		//EventManager.StartListening ("MovingPhase", changeOfPhaseListener);

		DragHandler.OnMove -= UserMove;
		DragHandler.OnMove += UserMove;

		playerSprites = Resources.LoadAll<Sprite>("chars");

		int p1Rand = UnityEngine.Random.Range(0, playerSprites.Length);

		int p2Rand = UnityEngine.Random.Range(0, playerSprites.Length);
		while (p2Rand == p1Rand)
		{
			p2Rand = UnityEngine.Random.Range(0, playerSprites.Length);
		}

		p1.GetComponent<Image>().sprite = playerSprites[p1Rand];
		p2.GetComponent<Image>().sprite = playerSprites[p2Rand];

		Slot.OnTileClicked -= UserPlace;
		Slot.OnTileClicked += UserPlace;
		Slot.OnTileClicked -= UserRemove;
	}

	private void StartGame()
	{
			string p1Name = randomNames[(new System.Random()).Next(0, randomNames.Count())];
			player1Name.text = p1Name;
			string p2Name = randomNames[(new System.Random()).Next(0, randomNames.Count())];
			while (p2Name == p1Name)
			{
				p2Name = randomNames[(new System.Random()).Next(0, randomNames.Count())];
			}
			player2Name.text = p2Name;
	}

	private IEnumerator ChangeTurn()
	{

		while (MoveObject.isMoving)
		{
			yield return new WaitForSeconds(0.1f);
		}

		changingTurn = true;

		StartCoroutine(UpdatePlayerTurnIndicatorDelayed ());

		changingTurn = false;

		player1Score.text = state.GetScore(1).ToString();
		player2Score.text = state.GetScore(2).ToString();

		if (state.IsTerminal())
		{
			ShowWinner();
		}
		else {
			logger.LogReadable(string.Format("Turn has changed. Now playing Player {0}", state.currentPlayer));

			if (state.remainingStonesToPlace == 0)
			{
				EnableDragAndDrop();
			}
		}
	}

	private void GamePhaseChanged()
	{
		logger.LogReadable("Placing game phase ended");
		if (state.remainingStonesToPlace == 0)
		{
			Slot.OnTileClicked -= UserPlace;
		}
	}

	private void UserPlace(GameObject tileClicked)
	{
		logger.LogReadable(string.Format("{0} clicked by User (Player {1})", tileClicked.transform.name, state.currentPlayer));

		int tileIndex = Util.GetIndexFromName(tileClicked.transform.name);

		if (TestForMill(tileIndex))
		{
			savedClickedTileIndexTo = tileIndex;
			MakeOnlyOpponentTilesClickable();

			PlaceToken(tileClicked.transform as RectTransform, tileIndex);
			StartCoroutine(ShowMillDelayed(tileClicked, tileIndex, state.currentPlayer));
			StartCoroutine(ShowTextDelayed("Great, you made a mill!\n Select a stone to remove."));
			DisableOpponentMills();

			//ShowMill (tileClicked, tileIndex, state.currentPlayer);

			Slot.OnTileClicked -= UserPlace;
			Slot.OnTileClicked += UserRemove;
		}
		else {
			PlaceToken(tileClicked.transform as RectTransform, tileIndex);
			StartCoroutine(ChangeTurn());
			savedClickedTileIndexTo = -1;
			var move = (GameMove)tileIndex;
			state.DoMove(move);
			MakeOnlyEmptyTilesClickable();
		}

	}

	private IEnumerator ShowTextDelayed(string s)
	{
		while (MoveObject.isMoving)
		{
			yield return new WaitForSeconds(0.1f);
		}

		text.text = s;
	}

	private void UserRemove(GameObject tileClicked)
	{
		Slot.OnTileClicked -= UserRemove;
		int removeIndex = -1;

		if (tileClicked.transform.childCount > 0)
		{

			removeIndex = Util.GetIndexFromName(tileClicked.transform.GetChild(0).transform.name);

			logger.LogReadable(string.Format("Player {0} removes token from {1}", state.currentPlayer, removeIndex));

			if (state.GetRemovableIndexes(state.opponentPlayer).Count > 0)
			{
				HideMill(state.currentPlayer);

				GameObject toDestroy = GameObject.Find(string.Format("Token {0}", removeIndex));
				GameObject exitPoint = GameObject.Find(string.Format("Exit {0}", UnityEngine.Random.Range(1, 7)));
				StartCoroutine(MoveObject.use.TranslateToTransform(toDestroy.transform.parent, exitPoint.transform, 0.75f, MoveObject.MoveType.Time, true));

				var move = new GameMove(savedClickedTileIndexFrom, savedClickedTileIndexTo, removeIndex);
				state.DoMove(move);

				// Removal can happen in placing phase and moving phase. As a consequence we want to make only empty tiles clickable in the first case
				// and only the tokens in the second case
				if (state.remainingStonesToPlace > 0)
				{
					Slot.OnTileClicked += UserPlace;
					MakeOnlyEmptyTilesClickable();
				}

				StartCoroutine(ChangeTurn());

			}
			else {
				logger.LogReadable("=!=\tCan't remove because part of mill");
			}
		}

	}

	private void UserMove(GameObject from, GameObject to)
	{
		GameObject startingTile = GameObject.Find(string.Format("Tile {0}", Util.GetIndexFromName(from.name)));

		int indexFrom = Util.GetIndexFromName(from.name);
		int indexTo = Util.GetIndexFromName(to.name);

		if (state.CanMoveToIndexes(indexFrom).Contains(indexTo))
		{

			RestoreInitialImageInAllTokens();

			logger.LogReadable(string.Format("Player {0} moving from from {1} to {2}", state.currentPlayer, indexFrom, indexTo));
			from.name = string.Format("Token {0}", indexTo);

			if (TestForMill(indexTo, indexFrom))
			{
				savedClickedTileIndexFrom = indexFrom;
				savedClickedTileIndexTo = indexTo;
				MakeOnlyOpponentTilesClickable();

				StartCoroutine(ShowMillDelayed(to, indexTo, state.currentPlayer));
				StartCoroutine(ShowTextDelayed("Great, you made a mill!\n Select token to remove."));

				//ShowMill (to, indexTo, state.currentPlayer);

				Slot.OnTileClicked += UserRemove;
			}
			else {

				// Update the business object with the move
				savedClickedTileIndexFrom = -1;
				savedClickedTileIndexTo = -1;
				var move = new GameMove(indexFrom, indexTo, -1);
				state.DoMove(move);
				StartCoroutine(ChangeTurn());
			}
		}
		else {
			logger.LogReadable(string.Format("=!=\tCan't move from {0} to {1}", indexFrom, indexTo));
			from.transform.SetParent(startingTile.transform);
		}
	}

	private bool TestForMill(int toIndex, int fromIndex = -1)
	{
		var tempState = state.Clone() as GameState;
		var currentPlayer = state.currentPlayer;

		if (fromIndex != -1)
		{
			tempState.DoMove(new GameMove(fromIndex, toIndex, -1));
		}
		else {
			tempState.DoMove(new GameMove(-1, toIndex, -1));
		}

		currentMills = tempState.MillsFromIndex(toIndex, currentPlayer);

		return currentMills.Any();
	}

	private void ShowWinner()
	{
		DisableAllTiles();
		DisableAllTokens(true);

		if (state.Winner() == 0)
		{
			text.text = "Draw";
			player1AvatarBackground.color = avatarNotPlayingColor;
			player2AvatarBackground.color = avatarNotPlayingColor;
		}
		else {
			text.text = string.Format("Player {0} wins", state.Winner());

			if (state.Winner() == 1)
			{
				text.text = string.Format("{0} wins", player1Name.text);
				// StartBlinking(player1AvatarBackground);
			}
			if (state.Winner() == 2)
			{
				text.text = string.Format("{0} wins", player2Name.text);
				// StartBlinking(player2AvatarBackground);
			}
		}
	}

	private IEnumerator Blink(Image image)
	{
		var counter = 0;
		while (true)
		{
			switch (counter)
			{
				case 0:
					image.color = new Color(avatarNotPlayingColor.r, avatarNotPlayingColor.g, avatarNotPlayingColor.b, 1);
					counter++;
					yield return new WaitForSeconds(0.5f);
					break;
				case 1:
					image.color = new Color(avatarWinnerColor.r, avatarWinnerColor.g, avatarWinnerColor.b, 1);
					counter--;
					yield return new WaitForSeconds(0.5f);
					break;
			}
		}
	}

	private void StartBlinking(Image image)
	{
		//StopAllCoroutines();
		StartCoroutine("Blink", image);
	}

	private void StopBlinking()
	{
		StopCoroutine("Blink");
	}

	IEnumerator ExecuteAfterTime(float time, Action functionToExecute /*Action<GameObject> functionToExecute, GameObject go*/)
	{
		yield return new WaitForSeconds(time);
		//functionToExecute.Invoke (go);
		functionToExecute.Invoke();
	}

	private IEnumerator ShowMillDelayed(GameObject tileClicked, int tileIndex, int player)
	{
		while (MoveObject.isMoving)
		{
			yield return new WaitForSeconds(0.1f);
		}

		HighlightMill(true, player);
		DisableOpponentMills();
	}

	private void ShowMill(GameObject tileClicked, int tileIndex, int player)
	{
		// Current player made a mill. Show text, place token, highlight mill and make tokens from opponent mills not clickable.
		text.text = "Great, you made a mill!\n Select a stone to remove.";
		PlaceToken(tileClicked.transform as RectTransform, tileIndex);
		HighlightMill(true, player);
		DisableOpponentMills();
	}

	private IEnumerator HideMillDelayed(int player)
	{
		while (MoveObject.isMoving)
		{
			yield return new WaitForSeconds(0.1f);
		}

		HideMill(player);
	}

	private void HideMill(int player)
	{
		text.text = string.Empty;
		HighlightMill(false, player);
		EnableMills();
	}

	private void MakeOnlyEmptyTilesClickable()
	{
		DisableAllTiles();
		EnableAllEmptyTiles();
	}

	private void MakeOnlyOpponentTilesClickable()
	{
		DisableAllTiles();
		DisableAllTokens();

		var opponentTokenTag = isPlayer1Playing() ? player2Token.tag : player1Token.tag;
		var opponentTokens = GameObject.FindGameObjectsWithTag(opponentTokenTag);

		foreach (GameObject opponentToken in opponentTokens)
		{
			opponentToken.GetComponent<Image>().raycastTarget = true;
		}
	}

	private void MakeOnlyCurrentPlayerTilesClickable()
	{
		DisableAllTiles();

		var currentPlayerTokenTag = isPlayer1Playing() ? player1Token.tag : player2Token.tag;
		var currentPlayerTokens = GameObject.FindGameObjectsWithTag(currentPlayerTokenTag);

		foreach (GameObject opponentToken in currentPlayerTokens)
		{
			opponentToken.GetComponent<Image>().raycastTarget = true;
		}
	}

	private void DisableAllTokens(bool makeTransparent = false)
	{
		foreach (GameObject tokenGo in GameObject.FindGameObjectsWithTag(player1Token.tag))
		{
			tokenGo.GetComponent<Image>().raycastTarget = false;
			if (makeTransparent)
			{
				tokenGo.GetComponent<Image>().color = tokenNotClickable;
			}
		}

		foreach (GameObject tokenGo in GameObject.FindGameObjectsWithTag(player2Token.tag))
		{
			tokenGo.GetComponent<Image>().raycastTarget = false;
			if (makeTransparent)
			{
				tokenGo.GetComponent<Image>().color = tokenNotClickable;
			}
		}
	}

	private void DisableAllTiles()
	{
		foreach (GameObject tileGo in GameObject.FindGameObjectsWithTag("Tile"))
		{
			tileGo.GetComponent<Image>().raycastTarget = false;
		}
	}

	private void EnableAllEmptyTiles()
	{
		var board = state.GetBoard();
		for (int boardPositionIndex = 0; boardPositionIndex < board.Length; boardPositionIndex++)
		{
			if (board[boardPositionIndex] == 0)
			{
				GameObject.Find(string.Format("Tile {0}", boardPositionIndex)).GetComponent<Image>().raycastTarget = true;
			}
		}
	}

	private void RestoreInitialImageInAllTokens()
	{
		foreach (GameObject tokenGo in GameObject.FindGameObjectsWithTag(player1Token.tag))
		{
			tokenGo.GetComponent<Image>().sprite = player1TokenNoHighlight;
		}

		foreach (GameObject tokenGo in GameObject.FindGameObjectsWithTag(player2Token.tag))
		{
			tokenGo.GetComponent<Image>().sprite = player2TokenNoHighlight;
		}
	}

	private void EnableDragAndDrop()
	{
		List<GameMove> moves = state.GetMoves();

		DisableAllTiles();
		DisableAllTokens();

		foreach (GameMove m in moves)
		{
			var token = GameObject.Find(string.Format("Token {0}", m.from));
			var tile = GameObject.Find(string.Format("Tile {0}", m.to));

			token.GetComponentInChildren<Image>().raycastTarget = true;
			token.GetComponent<Image>().sprite = isPlayer1Playing() ? player1TokenMove : player2TokenMove;
			tile.GetComponentInChildren<Image>().raycastTarget = true;
		}
	}

	private void PlaceToken(RectTransform selectedTilePosition, int index)
	{
		GameObject tileParent = GameObject.Find(string.Format("Tile {0}", index));

		if (state.remainingStonesToPlace > 0 && selectedTilePosition != null)
		{
			UpdatePlayerTokensLeftIndicator();

			if (isPlayer1Playing())
			{
				var randIndex = (new System.Random()).Next(0, player1TokensToPlace.Count);
				var p1Token = player1TokensToPlace.ElementAt(randIndex);
				(p1Token.transform as RectTransform).name = string.Format("Token {0}", index);
				StartCoroutine(MoveObject.use.TranslateToTransform(p1Token.transform, tileParent.transform, 0.75f, MoveObject.MoveType.Time));
				player1TokensToPlace.RemoveAt(randIndex);
			}
			else {
				var randIndex = (new System.Random()).Next(0, player2TokensToPlace.Count);
				var p2Token = player2TokensToPlace.ElementAt(randIndex);
				(p2Token.transform as RectTransform).name = string.Format("Token {0}", index);
				StartCoroutine(MoveObject.use.TranslateToTransform(p2Token.transform, tileParent.transform, 0.75f, MoveObject.MoveType.Time));
				player2TokensToPlace.RemoveAt(randIndex);
			}
		}
	}

	private IEnumerator UpdatePlayerTurnIndicatorDelayed()
	{
		while (!changingTurn)
		{
			yield return new WaitForSeconds(0.1f);
		}

		UpdatePlayerTurnIndicator();
	}

	private void UpdatePlayerTurnIndicator()
	{
        StartCoroutine(MoveObject.use.Rotation(turnIndicator.transform, new Vector3(0.0f, 0.0f, 180f), 0.35f));
	}

	private void UpdatePlayerTokensLeftIndicator()
	{
		if (isPlayer1Playing())
		{
			if (player1TokenLeftIndicators.Any())
			{
				player1TokenLeftIndicators.ElementAt((int)Math.Ceiling((double)(state.remainingStonesToPlace) / 2) - 1).GetComponent<Image>().color = tokenIndicatorUsed;
			}
		}
		else {
			if (player2TokenLeftIndicators.Any())
			{
				player2TokenLeftIndicators.ElementAt((int)Math.Ceiling((double)(state.remainingStonesToPlace) / 2) - 1).GetComponent<Image>().color = tokenIndicatorUsed;
			}

		}
	}

	private void HighlightMill(bool turnOn, int player)
	{
		var p1Token = turnOn ? player1TokenHighlight : player1TokenNoHighlight;
		var p2Token = turnOn ? player2TokenHighlight : player2TokenNoHighlight;

		foreach (int[] currentMill in currentMills)
		{
			foreach (int tokenIndex in currentMill)
			{
				var millTokenObject = GameObject.Find(string.Format("Token {0}", tokenIndex));
				millTokenObject.GetComponent<Image>().sprite = player == 1 ? p1Token : p2Token;
			}
		}
	}

	private void DisableOpponentMills()
	{
		var removableIndexes = state.GetRemovableIndexes(state.opponentPlayer);

		// there is at least one token that is not part of a mill and can be removed
		// for this reason we can disable the mills
		if (removableIndexes.Count > 0)
		{
			// get all the opponent tokens
			var opponentTokenTag = isPlayer1Playing() ? player2Token.tag : player1Token.tag;
			var opponentTokens = GameObject.FindGameObjectsWithTag(opponentTokenTag);

			deactivatedTiles = new List<GameObject>();
			deactivatedTokens = new List<GameObject>();

			foreach (GameObject go in opponentTokens)
			{
				int opponentTokenIndex;
				int.TryParse(go.name.Replace("Token ", ""), out opponentTokenIndex);
				GameObject tileGo;
				tileGo = GameObject.Find(string.Format("Tile {0}", opponentTokenIndex));

				if (!(removableIndexes.Contains(opponentTokenIndex)))
				{
					if (tileGo != null)
					{
						deactivatedTiles.Add(tileGo);
						deactivatedTokens.Add(go);
						tileGo.GetComponent<Image>().raycastTarget = false;
						go.GetComponent<Image>().color = tokenNotClickable;
						go.GetComponent<Image>().raycastTarget = false;
					}
				}
				else {
					tileGo.GetComponent<Image>().raycastTarget = true;
				}
			}
		}
	}

	private void EnableMills()
	{
		foreach (GameObject tokenGo in deactivatedTokens)
		{
			tokenGo.GetComponent<Image>().color = tokenClickable;
		}

		foreach (GameObject tileGo in deactivatedTiles)
		{
			tileGo.SetActive(true);
			tileGo.GetComponent<Button>().interactable = false;
		}

		deactivatedTokens = new List<GameObject>();
		deactivatedTiles = new List<GameObject>();
	}

	private bool isPlayer1Playing()
	{
		return state.currentPlayer % 2 != 0;
	}

	private void GatherTokenLeftIndicators()
	{
		player1TokenLeftIndicators = GameObject.FindGameObjectsWithTag("TokenBlackLeft").OrderByDescending(x => x.name).ToList();
		player2TokenLeftIndicators = GameObject.FindGameObjectsWithTag("TokenWhiteLeft").OrderByDescending(x => x.name).ToList();
	}

	private void GatherTokensToPlace()
	{
		player1TokensToPlace = GameObject.FindGameObjectsWithTag("TokenWhite").OrderByDescending(x => x.name).ToList();
		player2TokensToPlace = GameObject.FindGameObjectsWithTag("TokenBlack").OrderByDescending(x => x.name).ToList();
	}

	private void SetUpGame()
	{
		state = new GameState();
		state.OnGamePhaseChanged -= GamePhaseChanged;
		//state.Restart();
		state.OnGamePhaseChanged += GamePhaseChanged;
	}

	public void RestartGame()
	{
		StopBlinking();
		RestartGraphics();
		this.Start();
		this.StartGame();
		EnableAllEmptyTiles();
	}

	private void RestartGraphics()
	{
		foreach (GameObject tokenGo in GameObject.FindGameObjectsWithTag(player1Token.tag))
		{
			GameObject.Destroy(tokenGo);
		}

		foreach (GameObject tokenGo in GameObject.FindGameObjectsWithTag(player2Token.tag))
		{
			GameObject.Destroy(tokenGo);
		}

		player1TokenLeftIndicators.ForEach(p1Token => p1Token.SetActive(true));
		player2TokenLeftIndicators.ForEach(p2Token => p2Token.SetActive(true));
	}

	private void TestMove()
	{
		var fromToken = GameObject.Find(string.Format("Token {0}", 0));
		var toTile = GameObject.Find(string.Format("Tile {0}", 3));

		Debug.LogFormat("{0} position {1}", fromToken.name, fromToken.transform.localPosition);
		Debug.LogFormat("{0} local position {1}", fromToken.name, fromToken.transform.localPosition);

		Debug.LogFormat("{0} position {1}", fromToken.transform.parent.name, fromToken.transform.parent.position);
		Debug.LogFormat("{0} local position {1}", fromToken.transform.parent.name, fromToken.transform.parent.localPosition);

		Debug.LogFormat("{0} position {1}", toTile.name, toTile.transform.position);
		Debug.LogFormat("{0} local position {1}", toTile.name, toTile.transform.localPosition);

		//fromToken.transform.SetParent(fromToken.transform.parent.parent);

		Debug.LogFormat("{0} position {1}", fromToken.name, fromToken.transform.localPosition);
		Debug.LogFormat("{0} local position {1}", fromToken.name, fromToken.transform.localPosition);

		StartCoroutine(MoveObject.use.TranslateToTransform(fromToken.transform.parent, toTile.transform, 1f, MoveObject.MoveType.Time));

		Debug.LogFormat("{0} position {1}", fromToken.name, fromToken.transform.localPosition);
		Debug.LogFormat("{0} local position {1}", fromToken.name, fromToken.transform.localPosition);
	}
}