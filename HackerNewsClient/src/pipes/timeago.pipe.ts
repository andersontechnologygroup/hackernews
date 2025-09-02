import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeago'
})

export class TimeAgoPipe implements PipeTransform {
  transform(value: number, ...args: any[]): any {
    const now = new Date();
    const storyDate = new Date(value * 1000);
    let seconds = Math.floor((now.getTime() - storyDate.getTime()) / 1000);

    let resultString = "";

    const years = seconds / 31536000;
    if (years > 1) {
      resultString += Math.floor(years) + "y";
      seconds -= Math.floor(years) * 31536000;
    }

    const months = seconds / 2592000;
    if (months > 1) {
      if (resultString != "") resultString += ", ";
      resultString += Math.floor(months) + "mt";
      seconds -= Math.floor(months) * 2592000;
    }

    let days = seconds / 86400;
    if (days > 1) {
      if (resultString != "") resultString += ", ";
      resultString += Math.floor(days) + "d";
      seconds -= Math.floor(days) * 86400;
    }

    let hours = seconds / 3600;
    if (hours > 1) {
      if (resultString != "") resultString += ", ";
      resultString += Math.floor(hours) + "h";
      seconds -= Math.floor(hours) * 3600;
    }

    let minutes = seconds / 60;
    if (minutes > 1) {
      if (resultString != "") resultString += ", ";
      resultString += Math.floor(minutes) + "m";
      seconds -= Math.floor(minutes) * 60;
    }

    if (resultString != "") resultString += ", ";
    resultString += Math.floor(seconds) + "s";

    if (resultString != "") resultString += " ago";
    return resultString;
  }
}
