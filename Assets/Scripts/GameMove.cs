using System;

public class GameMove
{
	public int from;
	public int to;
	public int remove;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:AssemblyCSharp.GameMove"/> class.
	/// </summary>
	/// <param name="from">The index of the board position the player wants to move from</param>
	/// <param name="to">The index of the board position the player wants to move to</param>
	/// <param name="remove">The index of the board position the player wants to remove a stone from</param>
	public GameMove(int from, int to, int remove)
	{
		if (from < -1 || from >= GameState.NUMBER_OF_POSITIONS)
		{
			throw new ArgumentOutOfRangeException(string.Format("Trying to create an invalid move with \"from\" index of {0}", from));
		}

		if (to < -1 || to >= GameState.NUMBER_OF_POSITIONS)
		{
			throw new ArgumentOutOfRangeException(string.Format("Trying to create an invalid move with \"to\" index of {0}", to));
		}

		if (remove < -1 || remove >= GameState.NUMBER_OF_POSITIONS)
		{
			throw new ArgumentOutOfRangeException(string.Format("Trying to create an invalid move with \"remove\" index of {0}", remove));
		}

		this.from = from;
		this.to = to;
		this.remove = remove;
	}

	public static implicit operator int(GameMove move)
	{
		return move.to;
	}

	public static explicit operator GameMove(int moveTo)
	{
		return new GameMove(-1, moveTo, -1);
	}
}