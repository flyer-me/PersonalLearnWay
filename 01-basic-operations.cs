Console.WriteLine("Hello World!");    // Hello World
Console.WriteLine('b');    // b

Console.WriteLine(0.25F);    // 0.25 float
Console.WriteLine(2.625);    // 2.625 double
Console.WriteLine(12.39816m);    // decimal
/*
Float Type    Precision
float         ~6-9 digits
double        ~15-17 digits
decimal        28-29 digits
*/

Console.WriteLine(true);    //
Console.WriteLine(false);    //

char userOption;
int gameScore;
decimal particlesPerMillion;
bool processedCustomer;
string firstName = "Bob";
Console.WriteLine(firstName);

var message = "Hello world!";    //隐式类型本地变量,变量 message 会被立即设置为 string 值且不会更改
//var message;    // error CS0818: Implicitly-typed variables must be initialized
message = 10.703m;    // error CS0029: Cannot implicitly convert type 'decimal' to 'string'

Console.WriteLine("Hello\t\"World\"!");    // Hello    "World"!
Console.WriteLine("c:\\source\\repos");    // c:\source\repos
Console.Write(@"c:\invoices");    // c:\invoices
Console.WriteLine("\u3053\u3093\u306B\u3061\u306F World!");

int version = 11;
stringupdateText = "Update to ";
Console.WriteLine($"{updateText} {version}!");    // Update to 11!
stringprojectName = "First-Project";
Console.WriteLine($@"C:\Output\{projectName}\Data");    // C:\Output\First-Project\Data
