// Сортировка выбором
uses ArrayLib;

procedure SelectionSort(a: array of real);
begin
  for var i:=0 to a.Length-2 do
  begin
    var min := a[i]; 
    var ind := i;
    for var j:=i+1 to a.Length-1 do
      if a[j]<min then
      begin
        min := a[j];
        ind := j;
      end;
    a[ind] := a[i];
    a[i] := min;
  end;
end;

function CreateRandomArr(n: integer): array of real; 
begin
  Result := new real[n];
  for var i:=0 to Result.Length-1 do
    Result[i] := Random(100);
end;

var a: array of real;

begin
  a := CreateRandomArr(20);
  writeln('Содержимое массива: ');
  Writeln(a);
  SelectionSort(a);
  writeln('После сортировки выбором: ');
  Writeln(a);
end.