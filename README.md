# MASM-PlusPlus
Simple parser from C language constructions to MASM code (if else, for, while)
Example:

for (mov al, 0; al < 10; inc al)
{
  if (ah > 5)
  {
    mov dl, al
    add dl, 10
  }
    else
    {
      mov dl, al
      add dl, 35
    }
    print(al)
}

will turn into this:

mov al, 0
jmp __enter2
__for2:	;for (mov al, 0; al < 10; inc al)
    cmp ah, 5
  jle __else1	;if (ah > 5)
      mov dl, al
      add dl, 10
  jmp __end1
  __else1:	;else
      mov dl, al
      add dl, 35
  __end1:

  push ax
  push dx
  mov al, al
  mov ah, 02h
  mov dl, al
  int 21h;	        print(al)
  pop dx
  pop ax

  inc al
__enter2:
cmp al, 10
jl __for2
