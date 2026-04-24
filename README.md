# SGL Script

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-active%20prototype-0ea5e9)

Минималистичный интерпретируемый язык для игровой логики и алгоритмических сценариев.

SGL Script заточен под простую и читаемую запись правил: условия, циклы, массивы, диапазоны, базовая математика и встроенные функции. Проект написан на C# и состоит из классического пайплайна `Lexer -> Parser -> AST -> Interpreter`.

## Зачем этот проект

- Площадка для экспериментов с дизайном DSL и интерпретаторами.
- Компактная реализация языка, которую легко читать и расширять.
- Наглядные примеры алгоритмов: факториал, простые числа, сортировка.

## Возможности языка

- Типы: `number`, `string`, `bool`, `array`, `object`.
- Операторы: `+`, `-`, `*`, `/`, `%`, `>`, `<`, `=`, `and`, `or`, `not`.
- Управление потоком: `if / else`, `for ... in ...`, `break`, `return`.
- Диапазоны: `1..5`, `a..b`.
- Массивы: литералы, объединение/вычитание, методы коллекций.
- Встроенные функции: `Print`, `IsNumber`, `IsString`, `IsArray`, `IsBool`, `Sqrt`, `Floor`.
- Ввод аргументов через `with`:
	`with n: number, s: string, arr: array`.

## Стиль синтаксиса

- Разделитель инструкций - перевод строки. Одна строка = одна инструкция. Больше никаких забытых `;`.
- Блоки `if`, `else`, `for` закрываются фигурной скобкой `}`.
- Открывающая `{` не требуется: начало блока задается самой управляющей строкой (`if ...`, `for ...`, `else`).
- Типы переменных обычно выводятся из значения при присваивании (`x = 10`, `name = "text"`).
- Явное указание типа используется в `with` для входных аргументов (`with n: number, label: string`).

## Пример SGL-кода

Основной учебный пример находится в `sample.sgl` и показывает полный базовый стиль языка: аргументы через `with`, работу с массивами, арифметику и логику, `if/else`, диапазоны и циклы.

```sgl
# 1) Работа с массивом и вывод
with n: number, m: number, useRemainder: bool, label: string
# В примере ввод: 10 3 true "Result: "
Print "Input:", n, m, useRemainder, label
# Вывод: Input: 10 3 True Result:

numbers = [1, 2, 3]
numbers.Add n
count = numbers.Count
first = numbers.At 0

Print "Numbers:", numbers, "Count:", count
# Вывод: Numbers: [1, 2, 3, 10] Count: 4
Print "First:", first
# Вывод: First: 1

# 2) Базовые операции
sum = n + m
diff = n - m
mul = n * m
Print "sum:", sum, "diff:", diff, "mul:", mul
# Вывод: sum: 13 diff: 7 mul: 30

if useRemainder
    result = n % m
} else
    result = n / m
}
Print "result:", result
# Вывод: result: 1

isGreater = n > m
isEqual = n = m
logic = isGreater and not isEqual
Print "isGreater:", isGreater, "isEqual:", isEqual, "logic:", logic
# Вывод: isGreater: True isEqual: False logic: True

caption = "calc: " + label
Print caption + result
# Вывод: calc: Result: 1

# 3) Диапазон и вложенные условия
even = 0
divByThree = 0

for i in 1..n
    if i % 2 = 0
        even = even + 1
    } else
        if i % 3 = 0
            divByThree = divByThree + 1
        }
    }
}

Print "even:", even, "divByThree:", divByThree
# Вывод: even: 5 divByThree: 2

# 4) Цикл по массиву и проверка типа
for x in numbers
    if IsNumber x
        Print "value", x, "isEven", x % 2 = 0
    }
}
# Вывод по строкам:
# value 1 isEven False
# value 2 isEven True
# value 3 isEven False
# value 10 isEven True
```

Пример запуска:

```bash
dotnet run --project .\Sgl-script\Sgl-script.csproj .\sample.sgl 10 3 true "Result: "
```

## Быстрый старт

Требования:

- .NET SDK 9.0+

Запуск скрипта через `dotnet run`:

```bash
dotnet run --project .\Sgl-script\Sgl-script.csproj .\factorial.sgl 6
```

Ожидаемый вывод:

```text
720
```

Запуск готового `exe` (Windows):

```bat
run.bat factorial.sgl 6
```

## Примеры в репозитории

- `sample.sgl` - учебный сценарий с комментариями и обзор синтаксиса.
- `factorial.sgl` - вычисление факториала.
- `primes.sgl` - решето простых чисел.
- `sort.sgl` - сортировка массива.

## Архитектура

- `Lexer.cs` - токенизация исходного текста.
- `Parser.cs` - построение AST.
- `Ast.cs` - узлы синтаксического дерева.
- `Interpreter.cs` - выполнение программы.
- `ExecutionContext.cs` - регистрация функций/методов и аргументы запуска.
- `MemoryManager.cs` - области видимости и хранение переменных.

## Лицензия

Проект распространяется под лицензией MIT. См. `LICENSE`.
