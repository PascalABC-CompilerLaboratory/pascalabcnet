type A = class
  function Gen(n: integer): sequence of real;
  var j,k: real;
  begin
    var i := 222;
    begin
      var x := i;
    end;
    begin
      var x := j;
    end;
    for var n := 1 to 10 do
    begin
      yield i;
    end;
  end;
end;

begin
  var t := new A();
  foreach var x in t.Gen(5) do
    Print(x);
end.