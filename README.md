# g4

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which has been created via the `dotnet new giraffe` command.

## Build and test the application

### Windows

Run the `build.bat` script in order to restore, build and test (if you've selected to include tests) the application:

```
> ./build.bat
```

### Linux/macOS

Run the `build.sh` script in order to restore, build and test (if you've selected to include tests) the application:

```
$ ./build.sh
```

## Run the application

After a successful build you can start the web application by executing the following command in your terminal:

```
dotnet run -p src/g4
```

After the application has started visit [http://localhost:5000](http://localhost:5000) in your preferred browser.

POST /records example
```
POST https://localhost:5001/records
Content-Type: application/json

{"case":"Piped","fields":["smith|john|m|blue|12/25/1985"]}
```
where *case* can be Piped | Comma | Space

To start the console app taking input from files:
```
echo "smythe|jayne|green|12/25/1985" >/tmp/input.txt
dotnet run -p src/g4 --piped /tmp/input.txt

dotnet run -p src/g4 help

```