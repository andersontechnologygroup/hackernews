import { Component, OnInit, signal, computed, effect, inject, ChangeDetectionStrategy, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, of, tap, debounceTime, distinctUntilChanged, Subject } from 'rxjs';

// Data Model Interface
interface HackerNewsStory {
  id: number;
  title: string;
  url: string;
  by: string;
  score: number;
  time: number;
  postedAt: string; // Comes from API as DateTimeOffset
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: false,
  styleUrls: ['./app.component.css'],
  encapsulation: ViewEncapsulation.Emulated // Default and generally recommended
})

export class AppComponent implements OnInit {
  private readonly API_BASE_URL = 'https://localhost:7235'; // <-- IMPORTANT: Update with your C# API port
  private readonly PAGE_SIZE = 10
  private http = inject(HttpClient);
  private searchSubject = new Subject<void>();

  title = "Hacker News Client"

  // State Signals
  allStories = signal<HackerNewsStory[]>([]);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);
  
  // Search state
  searchKeyword = '';
  searchUser = '';

  // Pagination  state
  pageSize = signal<number>(this.PAGE_SIZE);
  currentPage = signal<number>(1);

  // Computed signals for Pagination
  totalPages = computed(() => {
    const total = this.allStories().length;
    const size = this.pageSize();
    return Math.ceil(total / size);
  });

  stories = computed(() => {
    const all = this.allStories();
    const page = this.currentPage() - 1;
    const size = this.pageSize();
    const start = page * size;
    const end = start + size;
    return all.slice(start, end);
  });

  constructor() {
    // Debounce search input
    this.searchSubject.pipe(
      debounceTime(400),
    ).subscribe(() => {
      this.performSearch();
    });
  }

  ngOnInit(): void {
    this.isLoading.set(false); // Not loading if not authenticated
  }

  // Data Fetching Methods
  getNewestStories(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http.get<HackerNewsStory[]>(`${this.API_BASE_URL}/api/v1/stories/newest`)
      .pipe(
        tap(stories => {
          this.allStories.set(stories);
          this.currentPage.set(1);
        }),
        catchError(err => this.handleApiError(err))
      )
      .subscribe(() => this.isLoading.set(false));
  }

  performSearch(): void {
    console.log("performing Search");

    const keyword = this.searchKeyword.trim();
    const user = this.searchUser.trim();

    console.log("keyword: " + keyword);
    console.log("user" + user);

    if (!keyword && !user) {
      this.getNewestStories();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    let params = new HttpParams();
    if (keyword) params = params.set('keyword', keyword);
    if (user) params = params.set('byUser', user);

    this.http.get<HackerNewsStory[]>(`${this.API_BASE_URL}/api/v1/stories/search`, { params })
      .pipe(
        tap(stories => {
          this.allStories.set(stories);
          this.currentPage.set(1);
        }),
        catchError(err => this.handleApiError(err))
      )
      .subscribe(() => this.isLoading.set(false));
  }

  onSearchChange(): void {
    this.searchSubject.next();
  }

  clearSearch(): void {
    this.searchKeyword = '';
    this.searchUser = '';
    this.getNewestStories();
  }

  goToNextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(page => page + 1);
    }
  }

  goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update(page => page - 1);
    }
  }

  getHostname(url: string): string {
    try {
      return new URL(url).hostname.replace('www.', '');
    } catch (e) {
      return '';
    }
  }

  private handleApiError(err: any) {
      this.errorMessage.set('Could not fetch stories. The API might be down.');

    this.allStories.set([]);
    return of(null); // Return an empty observable to complete the stream
  }
}

