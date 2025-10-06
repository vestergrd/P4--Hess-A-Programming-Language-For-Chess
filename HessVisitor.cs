using Antlr4.Runtime.Misc;
using Hess;
using System.Reflection;
using System.IO;
using System;

namespace Hess;

public class HessVisitor : HessBaseVisitor<object?>
{
    Dictionary<string, object?> bookOfWisdom = new Dictionary<string, object?>();
    char[] alphabet =
    {
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'I',
        'J',
        'K',
        'L',
        'M',
        'N',
        'O',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'U',
        'V',
        'W',
        'X',
        'Y',
        'Z'
    };

    public override object? VisitDefineBoard(HessParser.DefineBoardContext context)
    {
        var boardSize = context.BOARDPOSITION().GetText();
        UpdateElseAdd("BOARD", boardSize);
        return null;
    }

    public override object? VisitConstant(HessParser.ConstantContext context)
    {
        if (context.NATURAL_NUMBER() is { } i)
        {
            return int.Parse(i.GetText());
        }
        throw new NotImplementedException();
    }

    public override object? VisitAssignment(HessParser.AssignmentContext context)
    {
        //Movelist
        if (context.moveList() != null)
        {
            var moveListID = context.IDENTIFIER().GetText();
            var moveListValue = Visit(context.moveList());
            UpdateElseAdd(moveListID, moveListValue);
            return moveListValue;
        }
        //Player
        else if (context.player() != null)
        {
            var playerID = context.IDENTIFIER().GetText();
            var playerValue = Visit(context.player());
            UpdateElseAdd(playerID, playerValue);
            return playerValue;
        }
        //Expression
        else if (context.constant() != null)
        {
            var constantID = context.IDENTIFIER().GetText();
            var constantValue = Visit(context.constant());
            UpdateElseAdd(constantID, constantValue);
            return constantValue;
        }
        //Place
        else if (context.place() != null)
        {
            var placeID = context.IDENTIFIER().GetText();
            var placeValue = Visit(context.place());
            UpdateElseAdd(placeID, placeValue);

            return placeValue;
        }

        return null;
    }

    public override object? VisitMoveExtra(HessParser.MoveExtraContext context)
    {
        List<object?> moveExtraList = new List<object?>();
        if (context.NATURAL_NUMBER(1) is { } N)
        {
            moveExtraList.Add(context.NATURAL_NUMBER(0));
            moveExtraList.Add(Visit(context.direction()));
            moveExtraList.Add(context.NATURAL_NUMBER(1));
        }
        else if (context.direction() is { } D)
        {
            moveExtraList.Add(Visit(context.direction()));
            moveExtraList.Add(context.NATURAL_NUMBER());
        }
        else
        {
            moveExtraList.Add(context.NATURAL_NUMBER());
        }
        return moveExtraList;
    }

    public override object? VisitDirection(HessParser.DirectionContext context)
    {
        return context.GetText();
    }

    public override object? VisitAttacktype(HessParser.AttacktypeContext context)
    {
        return context.GetText();
    }

    public override object? VisitCollision(HessParser.CollisionContext context)
    {
        return context.GetText() == "true";
    }

    public override object? VisitMovetype(HessParser.MovetypeContext context)
    {
        return context.GetText();
    }

    public override object? VisitMove(HessParser.MoveContext context)
    {
        List<object?> move = new List<object?>();
        move.Add(Visit(context.movetype()));
        move.Add(Visit(context.collision()));
        move.Add(Visit(context.attacktype()));
        move.Add(Visit(context.direction()));

        if (context.moveExtra().ChildCount == 0)
        {
            throw new Exception("The move is incorrectly declared");
        }

        for (int i = 0; i < context.moveExtra().ChildCount; i++)
        {
            var moveExtraIndex = context.moveExtra().GetChild(i).GetText();

            if (int.TryParse(moveExtraIndex, out int j))
            {
                move.Add(int.Parse(moveExtraIndex));
            }
            else if (
                moveExtraIndex == "UP"
                || moveExtraIndex == "DOWN"
                || moveExtraIndex == "RIGHT"
                || moveExtraIndex == "LEFT"
            )
            {
                move.Add(moveExtraIndex);
            }
            else
            {
                var exception =
                    "The move " + "'" + context.GetText() + "'" + " is incorrectly declared";
                throw new Exception(exception);
            }
        }
        return move;
    }

    public override object? VisitMoveList(HessParser.MoveListContext context)
    {
        List<object?> moveList = new List<object?>();

        for (int i = 0; i < context.GetText().Split(',').Length; i++)
        {
            moveList.Add(Visit(context.move(i)));
        }
        return moveList;
    }

    public override object? VisitBoardpositionlist(HessParser.BoardpositionlistContext context)
    {
        return context.GetText();
    }

    public override object? VisitPlace(HessParser.PlaceContext context)
    {
        List<object?> placeList = new List<object?>();
        placeList.Add(context.IDENTIFIER().GetText());
        placeList.Add(Visit(context.boardpositionlist()));
        return placeList;
    }

    public override object? VisitPlaceType(HessParser.PlaceTypeContext context)
    {
        if (context.place() != null)
        {
            return Visit(context.place());
        }
        else
        {
            return context.IDENTIFIER().GetText();
        }
    }

    public override object? VisitPlayer(HessParser.PlayerContext context)
    {
        List<object?> playerList = new List<object?>();
        for (int i = 0; i < context.ChildCount / 2; i++)
        {
            playerList.Add(Visit(context.placeType(i)));
        }
        return playerList;
    }

    //BOOKOFWISDOM FUNCTIONS
    public bool UpdateElseAdd(string ID, object? value)
    {
        foreach (var element in bookOfWisdom)
        {
            if (ID == element.Key)
            {
                bookOfWisdom.Remove(element.Key);
                bookOfWisdom.Add(ID, value);
                return true;
            }
        }
        bookOfWisdom.Add(ID, value);
        return true;
    }

    public object? bookOfWisdomParser(string ID)
    {
        foreach (var element in bookOfWisdom)
        {
            if (ID == element.Key)
            {
                return element.Value;
            }
        }
        return null;
    }

    public int[] boardPosToValue(string? boardPos)
    {
        char letter = char.Parse(boardPos.Substring(0, 1));
        int letterValue = 1;
        foreach (char c in alphabet)
        {
            if (letter == c)
            {
                break;
            }
            letterValue++;
        }

        var boardValue = int.Parse(boardPos.Substring(1, boardPos.Length - 1));

        return new int[] { letterValue, boardValue };
    }

    public List<object> PlaceListUpdate(object place)
    {
        List<object> placeList = new List<object>();

        var placeCast = (List<object>)place;
        foreach (object obj in placeCast)
        {
            if (obj is string)
            {
                placeList.Add(bookOfWisdomParser(obj.ToString()));
            }
            else
            {
                placeList.Add(obj);
            }
        }
        return placeList;
    }

    Dictionary<string, List<int[]>> PlayerListParser(List<object> playerList)
    {
        Dictionary<string, List<int[]>> returnDic = new Dictionary<string, List<int[]>>();

        for (int i = 0; i < playerList.Count; i++)
        {
            List<int[]> boardPosList = new List<int[]>();
            List<object> typecasted = (List<object>)playerList[i];
            var piece_name = typecasted[0].ToString();
            var place_location_list = typecasted[1].ToString().Split('{', ',', '}');

            for (int j = 1; j < (place_location_list.Length - 1); j++)
            {
                boardPosList.Add(boardPosToValue(place_location_list[j]));
            }
            if (returnDic.ContainsKey(piece_name))
            {
                returnDic[piece_name] = returnDic[piece_name].Concat(boardPosList).ToList();
            }
            else
            {
                returnDic.Add(piece_name, boardPosList);
            }
        }

        return returnDic;
    }

    public string[,] NameAndPlace(
        string playerID,
        Dictionary<string, List<int[]>> dic,
        string[,] board
    )
    {
        foreach (var element in dic)
        {
            var indexer = 1;
            var coordinates = element.Value;

            foreach (var coordinate in coordinates)
            {
                var ID = playerID + "_" + element.Key + "_" + indexer;
                if (board[coordinate[0] - 1, coordinate[1] - 1] == "0")
                {
                    board[coordinate[0] - 1, coordinate[1] - 1] = ID;
                }
                else
                {
                    string exception =
                        "Attempted to place "
                        + element.Key
                        + " on ["
                        + coordinate[0]
                        + ","
                        + coordinate[1]
                        + "] which was already occupied by "
                        + board[coordinate[0], coordinate[1]];
                    throw new Exception(exception);
                }
                indexer++;
            }
        }

        return board;
    }

    public string[,] PlacePieces(string[,] board, object player, string playerID)
    {
        List<object> playerList1 = (List<object>)player;
        List<int[]> boardPosList = new List<int[]>();

        Dictionary<string, List<int[]>> P1PlaceLoc = PlayerListParser(playerList1);

        board = NameAndPlace(playerID, P1PlaceLoc, board);

        return board;
    }

    public void PrintBoard(string[,] board)
    {
        Console.Clear();
        for (int i = board.GetLength(1) + 1; i > 1; i--)
        {
            Console.Write("   ");
            for (int j = 1; j < board.GetLength(0) + 1; j++)
            {
                Console.Write("x--------");
            }
            Console.WriteLine("x");
            if ((i - 1) >= 10)
            {
                Console.Write(i - 1 + " |");
            }
            else
            {
                Console.Write(i - 1 + "  |");
            }
            for (int j = 1; j < board.GetLength(0) + 1; j++)
            {
                if (board[j - 1, i - 2] != "0")
                {
                    //HUSK AT GØRE DYNAMISK
                    if (board[j - 1, i - 2].Substring(0, 8) == "Spiller1")
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    else if (board[j - 1, i - 2].Substring(0, 8) == "Spiller2")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.Write(" {0} ", board[j - 1, i - 2].Substring(9, 6));
                    Console.ResetColor();
                    Console.Write("|");
                }
                else
                {
                    Console.Write("        |");
                }
            }
            Console.WriteLine();
        }
        Console.Write("   ");
        for (int j = 1; j < board.GetLength(0) + 1; j++)
        {
            Console.Write("x--------");
        }
        Console.Write("x");
        Console.WriteLine();

        string boardLetter = bookOfWisdomParser("BOARD").ToString().Substring(0, 1);
        char letter = char.Parse(boardLetter);

        Console.Write("        ");
        for (int i = 0; i < alphabet.Length; i++)
        {
            Console.Write(alphabet[i] + "        ");
            if (alphabet[i] == letter)
            {
                break;
            }
        }
        Console.WriteLine();
        Console.WriteLine();
    }

    public List<int[]>? GetMoves(
        List<object> moveList,
        int[] pieceLocation,
        string[,] board,
        Dictionary<string, int[]> ownPieces,
        Dictionary<string, int[]> opponentPieces,
        string getMovesType
    )
    {
        List<int[]> moveLocations = new List<int[]>();
        List<int[]> directTempLoc = new List<int[]>();
        int[] forLoopDir = new int[2];

        foreach (var move in moveList)
        {
            directTempLoc.Clear();
            var typecasted = (List<object>)move;
            var move_length = typecasted.Count;
            var move_type = typecasted[0].ToString();
            bool collision_type = (bool)typecasted[1];
            var attack_type = typecasted[2];

            string direction1 = "0";
            int direction1_int = 0;
            string direction2 = "0";
            int direction2_int = 0;

            bool notInAllPieces = true;
            bool noCollision = true;

            if (move_length == 5)
            {
                direction1 = typecasted[3].ToString();
                direction1_int = (int)typecasted[4];
                forLoopDir[0] = DirectionToForLoopInt(direction1);
            }
            else if (move_length == 6)
            {
                direction1 = typecasted[3].ToString();
                direction2 = typecasted[4].ToString();
                direction1_int = (int)typecasted[5];
                forLoopDir[0] = DirectionToForLoopInt(direction1);
                forLoopDir[1] = DirectionToForLoopInt(direction2);
            }
            else if (move_length == 7)
            {
                direction1 = typecasted[3].ToString();
                direction1_int = (int)typecasted[4];
                direction2 = typecasted[5].ToString();
                direction2_int = (int)typecasted[6];
                forLoopDir[0] = DirectionToForLoopInt(direction1);
                forLoopDir[1] = DirectionToForLoopInt(direction2);
            }

            if (
                (getMovesType == "notAll" && attack_type.ToString() != "MOVE")
                || getMovesType == "all"
            )
            {
                if (move_type == "Path" || move_type == "Direct")
                {
                    if (move_length == 5)
                    {
                        var pieceLocIndex = 0;
                        int[] finalLoc = new int[] { pieceLocation[0], pieceLocation[1] };
                        var locationCopy = new int[] { pieceLocation[0], pieceLocation[1] };

                        if (direction1 == "UP" || direction1 == "DOWN")
                        {
                            pieceLocIndex = 1;
                        }
                        finalLoc[pieceLocIndex] =
                            pieceLocation[pieceLocIndex] + (direction1_int * forLoopDir[0]);

                        for (
                            int i = pieceLocation[pieceLocIndex];
                            (IfPositiveOrNegative(i) && noCollision);

                        )
                        {
                            i += forLoopDir[0];
                            if (i > (board.GetLength(pieceLocIndex) - 1) || i < 0)
                            {
                                break;
                            }
                            locationCopy[pieceLocIndex] = i;

                            var boolList = CheckIfTaken(
                                ownPieces,
                                opponentPieces,
                                locationCopy,
                                notInAllPieces,
                                noCollision,
                                collision_type,
                                attack_type.ToString()
                            );

                            notInAllPieces = boolList[0];
                            noCollision = boolList[1];

                            if (
                                move_type == "Direct"
                                    && notInAllPieces
                                    && (attack_type.ToString() != "ATTACK")
                                || move_type == "Direct" && boolList[2]
                            )
                            {
                                directTempLoc.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }
                            else if (
                                notInAllPieces && (attack_type.ToString() != "ATTACK")
                                || boolList[2]
                            )
                            {
                                moveLocations.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }

                            notInAllPieces = true;
                        }
                        if (directTempLoc.Count != 0)
                        {
                            if (DirectCheck(directTempLoc, finalLoc))
                            {
                                moveLocations.Add(new int[] { finalLoc[0], finalLoc[1] });
                            }
                        }

                        bool IfPositiveOrNegative(int i)
                        {
                            if (forLoopDir[0] == 1)
                            {
                                if (i < (pieceLocation[pieceLocIndex] + direction1_int))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (forLoopDir[0] == -1)
                            {
                                if (i > (pieceLocation[pieceLocIndex] - direction1_int))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                    else if (move_length == 6)
                    {
                        var locationCopy = new int[] { pieceLocation[0], pieceLocation[1] };
                        var finalLoc = new int[]
                        {
                            pieceLocation[0] + (direction1_int * forLoopDir[1]),
                            pieceLocation[1] + (direction1_int * forLoopDir[0])
                        };
                        int j = pieceLocation[0];

                        for (int i = pieceLocation[1]; (IfPositiveOrNegative(i) && noCollision); )
                        {
                            i += forLoopDir[0];
                            j += forLoopDir[1];
                            if (
                                i > (board.GetLength(1) - 1)
                                || i < 0
                                || j > (board.GetLength(0) - 1)
                                || j < 0
                            )
                            {
                                break;
                            }
                            locationCopy[1] = i;
                            locationCopy[0] = j;

                            var boolList = CheckIfTaken(
                                ownPieces,
                                opponentPieces,
                                locationCopy,
                                notInAllPieces,
                                noCollision,
                                collision_type,
                                attack_type.ToString()
                            );

                            notInAllPieces = boolList[0];
                            noCollision = boolList[1];

                            if (
                                move_type == "Direct"
                                    && notInAllPieces
                                    && (attack_type.ToString() != "ATTACK")
                                || move_type == "Direct" && boolList[2]
                            )
                            {
                                directTempLoc.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }
                            else if (
                                notInAllPieces && (attack_type.ToString() != "ATTACK")
                                || boolList[2]
                            )
                            {
                                moveLocations.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }

                            notInAllPieces = true;
                        }
                        if (directTempLoc.Count != 0)
                        {
                            if (DirectCheck(directTempLoc, finalLoc))
                            {
                                moveLocations.Add(new int[] { finalLoc[0], finalLoc[1] });
                            }
                        }
                        bool IfPositiveOrNegative(int i)
                        {
                            if (forLoopDir[0] == 1)
                            {
                                if (i < (pieceLocation[1] + direction1_int))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (forLoopDir[0] == -1)
                            {
                                if (i > (pieceLocation[1] - direction1_int))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                    else if (move_length == 7)
                    {
                        var locationCopy = new int[] { pieceLocation[0], pieceLocation[1] };
                        var finalLoc = new int[]
                        {
                            pieceLocation[0] + (direction2_int * forLoopDir[1]),
                            pieceLocation[1] + (direction1_int * forLoopDir[0])
                        };
                        int i = 0;
                        int j = 0;

                        for (
                            i = pieceLocation[1];
                            (IfPositiveOrNegative(i, 1, direction1_int, 0) && noCollision);

                        )
                        {
                            i += forLoopDir[0];
                            if (i > (board.GetLength(1) - 1) || i < 0)
                            {
                                break;
                            }
                            locationCopy[1] = i;

                            var boolList = CheckIfTaken(
                                ownPieces,
                                opponentPieces,
                                locationCopy,
                                notInAllPieces,
                                noCollision,
                                collision_type,
                                attack_type.ToString()
                            );

                            notInAllPieces = boolList[0];
                            noCollision = boolList[1];

                            if (
                                move_type == "Direct"
                                    && notInAllPieces
                                    && (attack_type.ToString() != "ATTACK")
                                || move_type == "Direct" && boolList[2]
                            )
                            {
                                directTempLoc.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }
                            else if (
                                notInAllPieces && (attack_type.ToString() != "ATTACK")
                                || boolList[2]
                            )
                            {
                                moveLocations.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }

                            notInAllPieces = true;
                        }
                        for (
                            j = pieceLocation[0];
                            (IfPositiveOrNegative(j, 0, direction2_int, 1) && noCollision);

                        )
                        {
                            j += forLoopDir[1];
                            if (j > (board.GetLength(0) - 1) || j < 0)
                            {
                                break;
                            }
                            locationCopy[0] = j;

                            var boolList = CheckIfTaken(
                                ownPieces,
                                opponentPieces,
                                locationCopy,
                                notInAllPieces,
                                noCollision,
                                collision_type,
                                attack_type.ToString()
                            );

                            notInAllPieces = boolList[0];
                            noCollision = boolList[1];

                            if (
                                move_type == "Direct"
                                    && notInAllPieces
                                    && (attack_type.ToString() != "ATTACK")
                                || move_type == "Direct" && boolList[2]
                            )
                            {
                                directTempLoc.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }
                            else if (
                                notInAllPieces && (attack_type.ToString() != "ATTACK")
                                || boolList[2]
                            )
                            {
                                moveLocations.Add(new int[] { locationCopy[0], locationCopy[1] });
                            }

                            notInAllPieces = true;
                        }
                        if (directTempLoc.Count != 0)
                        {
                            if (DirectCheck(directTempLoc, finalLoc))
                            {
                                moveLocations.Add(new int[] { finalLoc[0], finalLoc[1] });
                            }
                        }

                        bool IfPositiveOrNegative(int x, int index, int direction, int posOrNeg)
                        {
                            if (forLoopDir[posOrNeg] == 1)
                            {
                                if (x < (pieceLocation[index] + direction))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (forLoopDir[posOrNeg] == -1)
                            {
                                if (x > (pieceLocation[index] - direction))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                }
            }
        }

        return moveLocations;
    }

    public bool DirectCheck(List<int[]> tempLocList, int[] finalLoc)
    {
        var count = tempLocList.Count;
        if (tempLocList[count - 1][0] == finalLoc[0] && tempLocList[count - 1][1] == finalLoc[1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<bool> CheckIfTaken(
        Dictionary<string, int[]> ownPieces,
        Dictionary<string, int[]> opponentPieces,
        int[] locationCopy,
        bool notInAllPieces,
        bool noCollision,
        bool collision_type,
        string attack_type
    )
    {
        bool add = false;

        foreach (var locationTaken in opponentPieces.Values)
        {
            if (locationTaken[0] == locationCopy[0] && locationTaken[1] == locationCopy[1])
            {
                notInAllPieces = false;
                if (attack_type.ToString() == "ATKMOVE" || attack_type.ToString() == "ATTACK")
                {
                    add = true;
                }
                if (collision_type is true)
                {
                    noCollision = false;
                }
                break;
            }
        }

        if (notInAllPieces)
        {
            foreach (var locationTaken in ownPieces.Values)
            {
                if (locationTaken[0] == locationCopy[0] && locationTaken[1] == locationCopy[1])
                {
                    notInAllPieces = false;
                    if (collision_type is true)
                    {
                        noCollision = false;
                    }
                    break;
                }
            }
        }
        return new List<bool> { notInAllPieces, noCollision, add };
    }

    public int DirectionToForLoopInt(string dir)
    {
        int intDir = 0;
        if (dir == "UP")
        {
            intDir = 1;
        }
        if (dir == "DOWN")
        {
            intDir = -1;
        }
        if (dir == "LEFT")
        {
            intDir = -1;
        }
        if (dir == "RIGHT")
        {
            intDir = 1;
        }
        return intDir;
    }

    public bool ValidateCheck(
        Dictionary<string, int[]> ownPieces,
        Dictionary<string, int[]> opponentPieces,
        string piece_name,
        int[] nextPosition,
        string[,] board,
        bool checkmateCase
    )
    {
        List<int[]>? enemyMoves = new List<int[]>();
        object? rough_movelist;
        List<int[]>? possibleMoves = new List<int[]>();

        var indexOne = nextPosition[0];
        var indexTwo = nextPosition[1];
        int[] previous;

        previous = new int[] { ownPieces[piece_name][0], ownPieces[piece_name][1] };

        ownPieces[piece_name][0] = indexOne;
        ownPieces[piece_name][1] = indexTwo;

        foreach (var piece in opponentPieces)
        {
            if (
                ownPieces[piece_name][0] != piece.Value[0]
                || ownPieces[piece_name][1] != piece.Value[1]
            )
            {
                rough_movelist = bookOfWisdomParser(piece.Key.Substring(0, piece.Key.Length - 2));
                possibleMoves = GetMoves(
                    (List<object>)rough_movelist,
                    opponentPieces[piece.Key],
                    board,
                    opponentPieces,
                    ownPieces,
                    "notAll"
                );

                foreach (var move in possibleMoves)
                {
                    enemyMoves.Add(new int[] { move[0], move[1] });
                }
            }
        }

        int[] kingPosition = ownPieces["KING_1"];

        foreach (var move in enemyMoves)
        {
            if (kingPosition[0] == move[0] && kingPosition[1] == move[1])
            {
                if (!checkmateCase)
                {
                    Console.WriteLine("King in check, try again..\n");
                }
                ownPieces[piece_name][0] = previous[0];
                ownPieces[piece_name][1] = previous[1];
                return false;
            }
        }

        if (checkmateCase)
        {
            ownPieces[piece_name][0] = previous[0];
            ownPieces[piece_name][1] = previous[1];
        }
        return true;
    }

    public bool ValidateCheckmate(
        string[,] board,
        string playerID,
        Dictionary<string, int[]> ownPieces,
        Dictionary<string, int[]> opponentPieces
    )
    {
        bool checkmate = false;
        object? rough_movelist;
        List<int[]>? possibleMoves = new List<int[]>();
        string pieceChecking = "";

        int kingCantMove = 0;
        int check_counter = 0;

        var kingMoveList = (List<object>)bookOfWisdomParser("KING");
        var kingLocation = ownPieces["KING_1"];

        foreach (var piece in opponentPieces)
        {
            rough_movelist = bookOfWisdomParser(piece.Key.Substring(0, piece.Key.Length - 2));
            possibleMoves = GetMoves(
                (List<object>)rough_movelist,
                opponentPieces[piece.Key],
                board,
                opponentPieces,
                ownPieces,
                "notAll"
            );

            foreach (var move in possibleMoves)
            {
                if (kingLocation[0] == move[0] && kingLocation[1] == move[1])
                {
                    check_counter++;
                    pieceChecking = piece.Key;
                    break;
                }
            }
        }

        if (check_counter == 1)
        {
            var king_moves = GetMoves(
                kingMoveList,
                kingLocation,
                board,
                ownPieces,
                opponentPieces,
                "all"
            );

            foreach (int[] kingMove in king_moves)
            {
                if (!ValidateCheck(ownPieces, opponentPieces, "KING_1", kingMove, board, true))
                {
                    kingCantMove++;
                }
            }

            if (kingCantMove != king_moves.Count)
            {
                return checkmate;
            }

            List<int[]>? ownMoves = new List<int[]>();
            int[] pieceCheckingLoc = opponentPieces[pieceChecking];

            foreach (var piece in ownPieces)
            {
                if (piece.Key == "KING_1")
                {
                    continue;
                }
                rough_movelist = bookOfWisdomParser(piece.Key.Substring(0, piece.Key.Length - 2));
                possibleMoves = GetMoves(
                    (List<object>)rough_movelist,
                    ownPieces[piece.Key],
                    board,
                    ownPieces,
                    opponentPieces,
                    "all"
                );

                foreach (var move in possibleMoves)
                {
                    ownMoves.Add(new int[] { move[0], move[1] });
                    if (move[0] == pieceCheckingLoc[0] && move[1] == pieceCheckingLoc[1])
                    {
                        return checkmate;
                    }
                }
            }

            var pieceCheckingMoveList =
                (List<object>)
                    bookOfWisdomParser(pieceChecking.Substring(0, pieceChecking.Length - 2));
            List<object> tempList = new List<object>();
            List<int[]> allCheckingPaths = new List<int[]>();
            List<int[]> intersectionLocations = new List<int[]>();
            int moveCheckCounter = 0;

            for (int i = 0; i < pieceCheckingMoveList.Count; i++)
            {
                tempList.Add(pieceCheckingMoveList[i]);
                possibleMoves = GetMoves(
                    tempList,
                    opponentPieces[pieceChecking],
                    board,
                    opponentPieces,
                    ownPieces,
                    "notAll"
                );

                foreach (int[] move in possibleMoves)
                {
                    if (move[0] == kingLocation[0] && move[1] == kingLocation[1])
                    {
                        if (((List<object>)pieceCheckingMoveList[i])[1] is false)
                        {
                            checkmate = true;
                            return checkmate;
                        }
                        else if (((List<object>)pieceCheckingMoveList[i])[1] is true)
                        {
                            foreach (int[] move2 in possibleMoves)
                            {
                                allCheckingPaths.Add(new int[] { move2[0], move2[1] });
                            }
                            moveCheckCounter++;
                        }
                    }
                }
                tempList.Clear();
            }

            if (moveCheckCounter == 1)
            {
                foreach (var move in ownMoves)
                {
                    foreach (var pathMove in allCheckingPaths)
                    {
                        if (move[0] == pathMove[0] && move[1] == pathMove[1])
                        {
                            return checkmate;
                        }
                    }
                }
                checkmate = true;
                return checkmate;
            }
            else if (moveCheckCounter > 1)
            {
                for (int i = 0; i < allCheckingPaths.Count; i++)
                {
                    int sameLocation = 1;
                    bool inList = false;
                    for (int j = 0; j < allCheckingPaths.Count; j++)
                    {
                        if (i != j)
                        {
                            if (
                                allCheckingPaths[i][0] == allCheckingPaths[j][0]
                                && allCheckingPaths[i][1] == allCheckingPaths[j][1]
                            )
                            {
                                sameLocation++;
                            }
                        }
                    }

                    if (sameLocation == moveCheckCounter)
                    {
                        foreach (var intersection in intersectionLocations)
                        {
                            if (
                                intersection[0] == allCheckingPaths[i][0]
                                && intersection[1] == allCheckingPaths[i][1]
                            )
                            {
                                inList = true;
                            }
                        }
                        if (!inList)
                        {
                            intersectionLocations.Add(
                                new int[] { allCheckingPaths[i][0], allCheckingPaths[i][1] }
                            );
                        }
                    }
                }

                foreach (var move in ownMoves)
                {
                    foreach (var intersectionMove in intersectionLocations)
                    {
                        if (move[0] == intersectionMove[0] && move[1] == intersectionMove[1])
                        {
                            return checkmate;
                        }
                    }
                }

                checkmate = true;
                return checkmate;
            }
        }
        else if (check_counter > 1)
        {
            var king_moves = GetMoves(
                kingMoveList,
                kingLocation,
                board,
                ownPieces,
                opponentPieces,
                "all"
            );

            foreach (int[] kingMove in king_moves)
            {
                if (!ValidateCheck(ownPieces, opponentPieces, "KING_1", kingMove, board, true))
                {
                    kingCantMove++;
                }
            }

            if (kingCantMove == king_moves.Count)
            {
                checkmate = true;
            }
        }

        return checkmate;
    }

    public bool ValidateInput(
        string player_input,
        string[,] board,
        string playerID,
        Dictionary<string, int[]> ownPieces,
        Dictionary<string, int[]> opponentPieces
    )
    {
        var player_inputs = player_input.Split('x', ' ');
        if (player_inputs.Length != 2)
        {
            Console.WriteLine("This input was not in the right format, try again..\n");
            return false;
        }
        var piece_name = player_inputs[0];
        var next_position = player_inputs[1];
        int[] nextPosition = boardPosToValue(next_position);
        object? rough_movelist;
        bool piece_exists = false;
        bool move_exists = false;
        List<int[]>? possibleMoves = new List<int[]>();

        foreach (var piece in ownPieces)
        {
            if (piece_name == piece.Key)
            {
                rough_movelist = bookOfWisdomParser(piece_name.Substring(0, piece_name.Length - 2));
                possibleMoves = GetMoves(
                    (List<object>)rough_movelist,
                    ownPieces[piece_name],
                    board,
                    ownPieces,
                    opponentPieces,
                    "all"
                );
                piece_exists = true;
                break;
            }
        }

        var nextPosIntArr = boardPosToValue(next_position);

        foreach (int[] move in possibleMoves)
        {
            if ((nextPosIntArr[0] - 1) == move[0] && (nextPosIntArr[1] - 1) == move[1])
            {
                move_exists = true;
                break;
            }
        }

        if (
            !piece_exists
            || !move_exists
            || !ValidateCheck(
                ownPieces,
                opponentPieces,
                piece_name,
                new int[] { (nextPosition[0] - 1), (nextPosition[1] - 1) },
                board,
                false
            )
        )
        {
            if (!piece_exists)
            {
                Console.WriteLine(
                    "The piece, {0}, does not exist or is already captured. Try again..\n",
                    piece_name
                );
            }
            else if (!move_exists)
            {
                Console.WriteLine(
                    "The move, {0} to {1}, is not valid..\nEither another piece is blocking, or the move is not within the moveset for {2}\n",
                    piece_name,
                    next_position,
                    piece_name
                );
            }

            return false;
        }

        return true;
    }

    public List<object> TakePlayerInput(
        string player1ID,
        string player2ID,
        string[,] board,
        int turn
    )
    {
        bool gameIsDone = false;
        bool moveIsCorrect = false;
        Dictionary<string, int[]> p1Pieces = new Dictionary<string, int[]>();
        Dictionary<string, int[]> p2Pieces = new Dictionary<string, int[]>();

        List<object> playerInputReturn = new List<object>();
        string playerInput;

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] != "0")
                {
                    var piece_values = board[i, j].Split('_', 2);
                    var player_name = piece_values[0];
                    var piece_name = piece_values[1];
                    if (player_name == player1ID)
                    {
                        p1Pieces.Add(piece_name, new int[] { i, j });
                    }
                    else if (player_name == player2ID)
                    {
                        p2Pieces.Add(piece_name, new int[] { i, j });
                    }
                }
            }
        }

        if ((turn % 2) == 1)
        {
            do
            {
                if (ValidateCheckmate(board, player1ID, p1Pieces, p2Pieces))
                {
                    Console.WriteLine("{0} won by checkmate", player2ID);
                    gameIsDone = true;
                    playerInputReturn.Add(board);
                    playerInputReturn.Add(gameIsDone);
                    return playerInputReturn;
                }
                Console.Write("{0} its your turn, make a move: ", player1ID);
                playerInput = Console.ReadLine();
                moveIsCorrect = ValidateInput(playerInput, board, player1ID, p1Pieces, p2Pieces);
            } while (!moveIsCorrect);

            var player_inputs = playerInput.Split('x', ' ');
            var piece_name = player_inputs[0];
            var next_position = player_inputs[1];
            int[] currentPosition = new int[2];
            int[] nextPosition = boardPosToValue(next_position);
            var indexOne = (nextPosition[0]) - 1;
            var indexTwo = (nextPosition[1]) - 1;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] != "0")
                    {
                        if (
                            player1ID == board[i, j].Substring(0, player1ID.Length)
                            && piece_name
                                == board[i, j].Substring((player1ID.Length + 1), piece_name.Length)
                        )
                        {
                            currentPosition[0] = i;
                            currentPosition[1] = j;
                        }
                    }
                }
            }

            board[currentPosition[0], currentPosition[1]] = "0";
            board[indexOne, indexTwo] = player1ID + "_" + piece_name;
        }
        else if ((turn % 2) == 0)
        {
            do
            {
                if (ValidateCheckmate(board, player2ID, p2Pieces, p1Pieces))
                {
                    Console.WriteLine("{0} won by checkmate", player1ID);
                    gameIsDone = true;
                    playerInputReturn.Add(board);
                    playerInputReturn.Add(gameIsDone);
                    return playerInputReturn;
                }
                Console.Write("{0} its your turn, make a move: ", player2ID);
                playerInput = Console.ReadLine();
                moveIsCorrect = ValidateInput(playerInput, board, player2ID, p2Pieces, p1Pieces);
            } while (!moveIsCorrect);

            var player_inputs = playerInput.Split('x', ' ');
            var piece_name = player_inputs[0];
            var next_position = player_inputs[1];
            int[] currentPosition = new int[2];
            int[] nextPosition = boardPosToValue(next_position);
            var indexOne = (nextPosition[0]) - 1;
            var indexTwo = (nextPosition[1]) - 1;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] != "0")
                    {
                        if (
                            player2ID == board[i, j].Substring(0, player2ID.Length)
                            && piece_name
                                == board[i, j].Substring((player2ID.Length + 1), piece_name.Length)
                        )
                        {
                            currentPosition[0] = i;
                            currentPosition[1] = j;
                        }
                    }
                }
            }

            board[currentPosition[0], currentPosition[1]] = "0";
            board[indexOne, indexTwo] = player2ID + "_" + piece_name;
        }
        playerInputReturn.Add(board);
        playerInputReturn.Add(gameIsDone);
        return playerInputReturn;
    }

    // STARTGAME / MAIN
    public override object? VisitStartGame(HessParser.StartGameContext context)
    {
        var player1ID = context.GetChild(1).GetText();
        var player2ID = context.GetChild(3).GetText();

        var player1 = bookOfWisdomParser(player1ID);
        var player2 = bookOfWisdomParser(player2ID);

        var boardDimensions = boardPosToValue(bookOfWisdomParser("BOARD")?.ToString());

        string[,] board = new string[boardDimensions[0], boardDimensions[1]];

        for (int i = 0; i < boardDimensions[0]; i++)
        {
            for (int j = 0; j < boardDimensions[1]; j++)
            {
                board[i, j] = "0";
            }
        }

        player1 = PlaceListUpdate(player1);
        player2 = PlaceListUpdate(player2);

        board = PlacePieces(board, player1, player1ID);
        board = PlacePieces(board, player2, player2ID);

        bool gameIsDone = false;
        int turn = 1;
        while (!gameIsDone)
        {
            PrintBoard(board);
            var playerInputReturn = TakePlayerInput(player1ID, player2ID, board, turn);
            board = (string[,])playerInputReturn[0];
            gameIsDone = (bool)playerInputReturn[1];
            turn++;
        }

        Console.ReadKey();
        return null;
    }
}
