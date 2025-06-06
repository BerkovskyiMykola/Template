# Template

`Template` serves as a template that can be used as an initial foundation for new projects. It provides best practices and a structured approach to service implementation.  

However, these best practices are not mandatory. If they do not suit the specific needs of a new project, they can be adjusted or replaced accordingly. The goal is to offer a useful starting point while allowing flexibility for project-specific requirements.

## NullGuard

This project uses **[NullGuard](https://github.com/Fody/NullGuard)** to automatically enforce null-safety at runtime. It injects null checks into method arguments and return values to catch potential `null` reference issues early.
