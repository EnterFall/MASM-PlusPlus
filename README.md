# MASM-PlusPlus
Simple parser from C language constructions to MASM code (if else, for, while)
Example:  
  
for (mov al, 0; al < 10; inc al)  
{  
&ensp;if (ah > 5)  
&ensp;{  
&ensp;&ensp;mov dl, al  
&ensp;&ensp;add dl, 10  
&ensp;}  
&ensp;else  
&ensp;{  
&ensp;&ensp;mov dl, al  
&ensp;&ensp;add dl, 35  
&ensp;}  
&ensp;print(al)  
}  
will turn into this:  
  
mov al, 0  
jmp __enter2  
__for2:	;for (mov al, 0; al < 10; inc al)  
&ensp;cmp ah, 5  
&ensp;jle __else1	;if (ah > 5)  
&ensp;&ensp;mov dl, al  
&ensp;&ensp;add dl, 10  
&ensp;jmp __end1  
&ensp;__else1:	;else  
&ensp;&ensp;mov dl, al  
&ensp;&ensp;add dl, 35  
&ensp;__end1:  
  
&ensp;push ax  
&ensp;push dx  
&ensp;mov al, al  
&ensp;mov ah, 02h  
&ensp;mov dl, al  
&ensp;int 21h;	        print(al)  
&ensp;pop dx  
&ensp;pop ax  

&ensp;inc al  
__enter2:  
cmp al, 10  
jl __for2  
 
