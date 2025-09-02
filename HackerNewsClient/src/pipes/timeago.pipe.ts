import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeago'
})

export class CustomPipe implements PipeTransform {
  transform(value: any, ...args: any[]): any {
    // Your pipe logic here
    //return modifiedValue;
  }
}
