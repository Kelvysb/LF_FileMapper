# LF_FileMapper
Customizable file mapper, search values in a source file and create a new file based on a pattern file.

Usage:

'''
 --run <mapper file name> [optional directory path]
     --replace (optional replace existing files)

Initialize mapper:
 --init <mapper file name> [mapper file path optional]
     --interactive (optional inform values)

Open Mapper Files:
 --open [Mapper name optional]

 Get current dir:
     -env or --env

 Program Version:
     -v or --version
     
 Help:
     -h or --help
'''

Mapper File Example:
This file is used to gather variables from source file, filter directories and files to search and configure the output file name and path, it uses regular expressions fo search content and filter.

the "SearchLocation" can be:

0 - Content

1 - FileName

2 - FileDirectory

3 - FilePath


the "OutputPatern" points to a .txt contaning the output patern with the variables notation.

```
{
    "Name": "test",
    "Variables": [
        {
            "Name": "class",
            "SearchPatern": "(?<=class\\s).[a-zA-Z0-9]*",
            "SearchLocation": 0
        },
        {
            "Name": "method",
            "SearchPatern": "(?<!(new)\\s)(?<=.\\s).[a-zA-Z0-9]*(?=\\b\\()",
            "SearchLocation": 0
        }
    ],
    "OutputFolder": ".\\",
    "OutputFileName": "<%class%>Test.cs",
    "OutputPatern": "test.txt",
    "DirFilter": "",
    "FileFilter": "(?=(\\.cs)$)",
    "DirExclude": "",
    "FileExclude": "test"
}
```

Pattern file example:

Variables can be used like this:

<%name%> = variable (simple variable replace)

<@name@> = repeat for each match of variable in source file (begin of block) 

<!name!> = value of current item in loop

</@name@> = repeat for each match of variable in source file (end of block)


```
class <%class%>
{
    <@method@> //This block will repeat for each occourence of a pattern on source file.
    private void <!method!>(string arg)
    {
    
    }
    </@method@>
}
```


