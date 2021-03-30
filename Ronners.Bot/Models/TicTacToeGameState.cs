using Discord;
using System.Linq;


namespace Ronners.Bot.Models
{
    public class TicTacToeGameState : GameState
    {
        private ulong PlayerOneId;
        private ulong PlayerTwoId;
        private char[] GameBoard;
        public int GameComplete{get;set;}
        private ulong CurrentPlayerTurn;
        public TicTacToeGameState(IUserMessage message, ulong playerOne, ulong playerTwo) : base(message)
        {
            PlayerOneId = playerOne;
            PlayerTwoId = playerTwo;
            GameBoard = new char[9]{'1', '2', '3', '4', '5', '6', '7', '8', '9'};
            GameComplete= -1;
            CurrentPlayerTurn = playerTwo;
        }
        public bool Update(IEmote reaction, ulong userId)
        {
            bool updated =false;
            char nextMove;
            
            //Wrong Player reaction
            if(CurrentPlayerTurn != userId)
                return false;
            
            //invalid move
            int move = GetMove(reaction);
            if(move > 8)
                return false;
            if(GameBoard[move]=='X' || GameBoard[move] == 'O')
                return false;

            if(userId == PlayerOneId)
                nextMove = 'O';
            else
                nextMove = 'X';

            GameBoard[move] = nextMove;
            updated =true;

            GameComplete = GameOver();

            if(CurrentPlayerTurn == PlayerOneId)
                CurrentPlayerTurn = PlayerTwoId;
            else
                CurrentPlayerTurn = PlayerOneId;
            return updated;
        }

        private bool RowCross()
        {
            if((GameBoard[0] == 'X'|| GameBoard[0] == 'O') && GameBoard[0] == GameBoard[1] && GameBoard[1] == GameBoard[2])
                return true;
            if((GameBoard[1] == 'X'|| GameBoard[1] == 'O') && GameBoard[1] == GameBoard[4] && GameBoard[4] == GameBoard[7])
                return true;
            if((GameBoard[6] == 'X'|| GameBoard[6] == 'O') && GameBoard[6] == GameBoard[7] && GameBoard[7] == GameBoard[8])
                return true;
            return false;
        }
        private bool ColumnCross()
        {
            if((GameBoard[0] == 'X'|| GameBoard[0] == 'O') && GameBoard[0] == GameBoard[3] && GameBoard[3] == GameBoard[6])
                return true;
            if((GameBoard[3] == 'X'|| GameBoard[3] == 'O') && GameBoard[3] == GameBoard[4] && GameBoard[4] == GameBoard[5])
                return true;
            if((GameBoard[2] == 'X'|| GameBoard[2] == 'O') && GameBoard[2] == GameBoard[5] && GameBoard[5] == GameBoard[8])
                return true;
            return false;
        }
        private bool DiagonalCross()
        {
            if((GameBoard[0] == 'X'|| GameBoard[0] == 'O') && GameBoard[0] == GameBoard[4] && GameBoard[4] == GameBoard[8])
                return true;
            if((GameBoard[2] == 'X'|| GameBoard[2] == 'O') && GameBoard[2] == GameBoard[4] && GameBoard[4] == GameBoard[6])
                return true;
            return false;
        }
        private bool FullBoard()
        {
            return GameBoard.All(x=> x=='X'||x=='O');
        }

        private int GameOver()
        {
            if(RowCross() || ColumnCross() || DiagonalCross())
                return 1;
            else if(FullBoard())
                return 0;
            else
                return -1;
        }

        private int GetMove(IEmote reaction)
        {
            switch(reaction.Name)
            {
                case "\u0031\uFE0F\u20E3":
                    return 0;
                case "\u0032\uFE0F\u20E3":
                    return 1;
                case "\u0033\uFE0F\u20E3":
                    return 2;
                case "\u0034\uFE0F\u20E3":
                    return 3;
                case "\u0035\uFE0F\u20E3":
                    return 4;
                case "\u0036\uFE0F\u20E3":
                    return 5;
                case "\u0037\uFE0F\u20E3":
                    return 6;
                case "\u0038\uFE0F\u20E3":
                    return 7;
                case "\u0039\uFE0F\u20E3":
                    return 8;
                default:
                    return 10;
            }
        }
        public override string ToString()
        {
            string Header ="";
            if(GameComplete == -1)
                Header= $"Turn <@{CurrentPlayerTurn}>";
            else if(GameComplete == 0)
                Header ="Tie";
            else if(GameComplete == 1)
                if(CurrentPlayerTurn == PlayerOneId)
                    Header = $"<@{PlayerTwoId}> Wins!";
                else    
                    Header = $"<@{PlayerOneId} Wins!>";
            return 
$@"{Header}``` 
 {GameBoard[0]} │ {GameBoard[1]} │ {GameBoard[2]} 
───┼───┼───
 {GameBoard[3]} │ {GameBoard[4]} │ {GameBoard[5]} 
───┼───┼───
 {GameBoard[6]} │ {GameBoard[7]} │ {GameBoard[8]} 
```";
        }
    }
}