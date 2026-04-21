# EasySave

EasySave is a backup software project developed for **ProSoft**, a software publishing company.  
The project is managed under the responsibility of the CIO and follows the technical, documentation, and maintainability standards defined for all ProSoft Suite applications.


## Project Overview

The goal of EasySave is to provide a reliable, maintainable, and user-friendly backup solution for end users.

The development team is responsible for:

- Designing and developing the software.
- Managing major and minor releases.
- Writing and maintaining all required documentation.
- Ensuring the project can be easily taken over by another team in the future.
- Reducing the development cost of future versions.
- Allowing fast diagnosis and correction of software issues.

## Technical Stack

The project must be developed using the following technologies and tools:

- **Language:** C#
- **Framework:** .NET Framework 4.8.1
- **IDE:** Visual Studio 2022 or higher
- **Version Control:** GitHub
- **UML Modeling Tool:** MERMAID

## Structure

- Main branch or the Production branch: `main`
This branch is used for the production version of the software. 
It should always contain stable and tested code that is ready for release. 

- PreProduction branch: `PreProduction`
This branch is used for testing and validation before merging into the main branch. 
All the code that is ready, tested, and validated in the DEV branch can centralize into the PreProduction branch to have a reliable base AND continue adding **Hotfix** only.

- Development branch: `DEV`
This branch is used for active development. The development team works on new features, bug fixes, and improvements in this branch. Everyone starts from this branch to create their own branches for specific features or bug fixes.
All will be centralized to test if the core functionalities are still working before merging into the PreProduction branch.