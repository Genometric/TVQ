Thanks for showing interest to contribute to TVQ 💖

When it comes to open-source, every contribution you 
make, makes the software better for everyone, and 
that is extensively valuable and warmly appreciated 
by the community. To express our gratitude for your 
contribution, we do our best to provide you with 
easy-to-follow steps to get started. 

##Repository Structure
This repository has three main branches:

- [master](https://github.com/Genometric/TVQ/tree/master);
this is the branch where the source code resides. 
- [docs](https://github.com/Genometric/TVQ/tree/docs);
this branch only contains the material used to generate
the [tvq website](https://genometric.github.io/TVQ/).
- [gh-pages](https://github.com/Genometric/TVQ/tree/gh-pages);
this branch contains the static website generated from 
the material on the [docs](https://github.com/Genometric/TVQ/tree/docs)
branch. The content of this branch is auto-generated 
once a commit is pushed to the [docs](https://github.com/Genometric/TVQ/tree/docs)
branch. 

##Basics of git

_If you are familiar with the basics of `git` you may skip this section;
if you are not, this section provides you with few steps you may take to get
started. You may also refer to [this basic git commands](https://guides.github.com/introduction/git-handbook/#basic-git) 
for more details._

All the changes to the repository are made via 
[pull requests (PR)](https://docs.github.com/en/free-pro-team@latest/github/collaborating-with-issues-and-pull-requests/about-pull-requests);
you may take following steps to make a change and submit a PR.

1. Fork the repository by clicking on the <kbd>Fork</kbd> 
on the top-right corner on the 
[repository's github page](https://github.com/Genometric/TVQ).

2. Open your favorite shell/terminal, `cd` to the path where 
you want to [`clone`](https://git-scm.com/docs/git-clone) the TVQ code, and run: 

    ```shell script
    $ git clone https://github.com/<YOUR_GITHUB_USERNAME>/tvq.git .
    ``` 

3. [`checkout`](https://git-scm.com/docs/git-checkout) the branch:

    3.a. if you want to contribute to the source code:
    
    ```shell script
    $ git checkout master
    ```  
    (`master` is the default branch, so it will checked-out by default after `clone`.)
    
    3.b. if you want to contribute to the documentation:
    
    ```shell script
    $ git checkout docs
    ``` 

4. Make the change you want, and commit them. First you may run the following to make sure 
that the changes you made are "tracked": 

    ```shell script
    $ git status
    ```
     
    if this command outputs a message like the following, that means
    the changes you made are _not_ tracked: 
     
    ```shell script
    On branch master
    Your branch is up to date with 'origin/master'.
     
    Untracked files:
      (use "git add <file>..." to include in what will be committed)
        I_am_not_tracked.txt
    
    nothing added to commit but untracked files present (use "git add" to track)
    ```
    Here the file `I_am_not_tracked.txt` is not tracked, and to track it, you may
    run the following: 
    
    ```shell script
    $ git add I_am_not_tracked.txt
    ```
    Then to commit the changes, you may run: 
    
    ```shell script
    $ git commit -m "A descriptive message for the changes you made."
    ```
    
5. The changes are now committed to git, but are stored on your machine only, 
to send them to Github, you may run the following:

    ```shell script
    $ git push
    ```

6. [Submit a pull request](https://docs.github.com/en/free-pro-team@latest/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request). 
   

##Getting Started

This repository contains multiple projects written in different 
programming languages (e.g., ASP.NET/C#, Python, and R), and each 
project executes a unique tasks. The components are: 

- Collect information about the tools; implemented using different scripts
each written a programming language that best matches the package management system.

    **[Getting started with the Crawlers](https://genometric.github.io/TVQ/docs/offline_crawlers/getting_started)**

- Aggregate collected information and search for scholarly information, and 
perform preliminary statistical analysis; implemented in ASP.NET C# and is 
available as a webservice with API-based access.

    **[Getting started with the Webservice](https://genometric.github.io/TVQ/docs/webservice/getting_started)**

- Statistical analysis and plotting; implemented via multiple python scripts.

    **[Getting started with the Statistical Analysis Scripts](https://genometric.github.io/TVQ/docs/analytics/getting_started)**
 
## License
By contributing your code to the TVQ GitHub repository, 
you agree to license your contribution under the MIT license.
   
   