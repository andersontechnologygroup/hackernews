import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { AppComponent } from './app.component';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TimeAgoPipe } from "../pipes/timeago.pipe";

// Define the story interface locally for testing purposes
interface HackerNewsStory {
  id: number;
  title: string;
  url: string;
  by: string;
  score: number;
  time: number;
  postedAt: string;
}

// --- MOCK DATA ---
/*
const MOCK_STORIES: HackerNewsStory[] = [
  { id: 1, title: 'Angular Testing Guide', url: 'https://angular.dev/guide/testing', by: 'testuser', score: 150, time: Math.floor(Date.now() / 1000) - 3600, postedAt: new Date().toISOString() },
  { id: 2, title: 'RxJS in Practice', url: 'https://rxjs.dev', by: 'anotheruser', score: 200, time: Math.floor(Date.now() / 1000) - 7200, postedAt: new Date().toISOString() },
];
*/
const MOCK_STORIES: HackerNewsStory[] = [
  { id: 1, title: 'Story 1', url: 'https://valid.com/story1', by: 'user1', score: 150, time: Math.floor(Date.now() / 1000) - 3600, postedAt: new Date().toISOString() },
  { id: 4, title: 'Story 4', url: 'https://valid.com/story4', by: 'user4', score: 200, time: Math.floor(Date.now() / 1000) - 3597, postedAt: new Date().toISOString() },
  { id: 5, title: 'Story 5', url: 'https://valid.com/story5', by: 'user1', score: 200, time: Math.floor(Date.now() / 1000) - 3596, postedAt: new Date().toISOString() },
  { id: 7, title: 'Story 7', url: 'https://valid.com/story7', by: 'user7', score: 200, time: Math.floor(Date.now() / 1000) - 3594, postedAt: new Date().toISOString() },
  { id: 8, title: 'Story 8', url: 'https://valid.com/story8', by: 'user8', score: 200, time: Math.floor(Date.now() / 1000) - 3593, postedAt: new Date().toISOString() },
  { id: 9, title: 'Story 9', url: 'https://valid.com/story9', by: 'user9', score: 200, time: Math.floor(Date.now() / 1000) - 3592, postedAt: new Date().toISOString() },
  { id: 10, title: 'Story 10', url: 'https://valid.com/story10', by: 'user10', score: 200, time: Math.floor(Date.now() / 1000) - 3591, postedAt: new Date().toISOString() },
  { id: 11, title: 'Story 11', url: 'https://valid.com/story11', by: 'user11', score: 200, time: Math.floor(Date.now() / 1000) - 3590, postedAt: new Date().toISOString() },
  { id: 12, title: 'Story 12', url: 'https://valid.com/story12  ', by: 'user12', score: 200, time: Math.floor(Date.now() / 1000) - 3589, postedAt: new Date().toISOString() },
  { id: 13, title: 'Story 13', url: 'https://valid.com/story13', by: 'user13', score: 200, time: Math.floor(Date.now() / 1000) - 3588, postedAt: new Date().toISOString() }
];

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let nativeElement: HTMLElement;
  let httpTestingController: HttpTestingController;

  const API_BASE_URL = 'https://localhost:7235';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        FormsModule,
        RouterOutlet,
        CommonModule,
        TimeAgoPipe
      ],
      declarations: [

        AppComponent // Import the standalone component directly
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Verify that there are no outstanding HTTP requests after each test
    httpTestingController.verify();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization (ngOnInit)', () => {
    it('should fetch stories ', () => {
      fixture.detectChanges();

      httpTestingController.expectNone(`${API_BASE_URL}/api/v1/stories/newest`);

      // There isn't supposed to be a call at the start of OnInit, but once we
      // clear the search, it should call it.
      component.clearSearch();

      const req = httpTestingController.expectOne(`${API_BASE_URL}/api/v1/stories/newest`);
      expect(req.request.method).toBe('GET');
      req.flush(MOCK_STORIES);

      expect(component.allStories().length).toBe(10);
      expect(component.isLoading()).toBe(false);

      fixture.detectChanges();

      const storyCards = nativeElement.querySelectorAll('.story-card');
      expect(storyCards[0].textContent).toContain('Story 1');
      expect(storyCards[1].textContent).toContain('Story 4');
      expect(storyCards[2].textContent).toContain('Story 5');
      expect(storyCards[3].textContent).toContain('Story 7');
      expect(storyCards[4].textContent).toContain('Story 8');
      expect(storyCards[5].textContent).toContain('Story 9');
      expect(storyCards[6].textContent).toContain('Story 10');
      expect(storyCards[7].textContent).toContain('Story 11');
      expect(storyCards[8].textContent).toContain('Story 12');
      expect(storyCards[9].textContent).toContain('Story 13');
    });
  });

  describe('Data Fetching and Display', () => {
    it('should display stories after a successful fetch', () => {
      component.allStories.set(MOCK_STORIES);
      component.isLoading.set(false);
      fixture.detectChanges();

      const storyCards = nativeElement.querySelectorAll('.story-card');
      expect(storyCards.length).toBe(10);
      expect(storyCards[0].textContent).toContain('Story 1');
      expect(storyCards[1].textContent).toContain('Story 4');
      expect(storyCards[2].textContent).toContain('Story 5');
      expect(storyCards[3].textContent).toContain('Story 7');
      expect(storyCards[4].textContent).toContain('Story 8');
      expect(storyCards[5].textContent).toContain('Story 9');
      expect(storyCards[6].textContent).toContain('Story 10');
      expect(storyCards[7].textContent).toContain('Story 11');
      expect(storyCards[8].textContent).toContain('Story 12');
      expect(storyCards[9].textContent).toContain('Story 13');
    });

    it('should display an error message on API failure', () => {
      component.getNewestStories();
      const req = httpTestingController.expectOne(`${API_BASE_URL}/api/v1/stories/newest`);
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      fixture.detectChanges();
      expect(component.errorMessage()).toBe('Could not fetch stories. The API might be down.');
      const errorDiv = nativeElement.querySelector('[role="alert"]');
      expect(errorDiv?.textContent).toContain('An Error Occurred!');
    });

    it('should display "No Stories Found" message when search returns empty', () => {
      component.isLoading.set(false);
      component.allStories.set([]);
      fixture.detectChanges();

      const emptyMessage = nativeElement.querySelector('.text-center h3');
      expect(emptyMessage?.textContent).toBe('No Stories Found');
    });
  });

  describe('Search Functionality', () => {
    it('should perform a search with debounce', fakeAsync(() => {
      component.searchKeyword = 'story';
      component.onSearchChange();

      // Nothing happens immediately
      httpTestingController.expectNone(`${API_BASE_URL}/api/v1/stories/search?keyword=story`);

      tick(200); // Advance time a little

      httpTestingController.expectNone(`${API_BASE_URL}/api/v1/stories/search?keyword=story`);

      tick(200);

      const req = httpTestingController.expectOne(`${API_BASE_URL}/api/v1/stories/search?keyword=story`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    }));

    it('should call getNewestStories when search is cleared', () => {
      component.searchKeyword = 'test';
      component.clearSearch();

      const req = httpTestingController.expectOne(`${API_BASE_URL}/api/v1/stories/newest`);
      expect(req.request.method).toBe('GET');
      req.flush([]);

      expect(component.searchKeyword).toBe('');
      expect(component.searchUser).toBe('');
    });
  });

  describe('Pagination', () => {
    beforeEach(() => {
      const manyStories = Array.from({ length: 25 }, (_, i) => ({
        id: i + 1, title: `Story ${i + 1}`, url: `https://valid.com/story${i + 1}`, by: `user${i + 1}`, score: 10, time: 0, postedAt: ''
      }));
      component.allStories.set(manyStories);
      component.pageSize.set(10);
      fixture.detectChanges();

    });

    it('should calculate total pages correctly', () => {
      expect(component.totalPages()).toBe(3);
    });

    it('should display the first page of stories initially', () => {
      expect(component.stories().length).toBe(10);
      expect(component.stories()[0].title).toBe('Story 1');
    });

    it('should navigate to the next page', () => {
      component.currentPage.set(1);
      component.goToNextPage();
      fixture.detectChanges();

      expect(component.currentPage()).toBe(2);
      expect(component.stories().length).toBe(10);
      expect(component.stories()[0].title).toBe('Story 11');

      component.goToNextPage();
      fixture.detectChanges();

      expect(component.currentPage()).toBe(3);
      expect(component.stories().length).toBe(5);
      expect(component.stories()[0].title).toBe('Story 21');
    });

    it('should navigate to the previous page', () => {
      component.currentPage.set(3);
      component.goToPreviousPage();
      fixture.detectChanges();

      expect(component.currentPage()).toBe(2);
      expect(component.stories()[0].title).toBe('Story 11');

      component.goToPreviousPage();
      fixture.detectChanges();

      expect(component.currentPage()).toBe(1);
      expect(component.stories()[0].title).toBe('Story 1');
    });

    it('should disable the "Previous" button on the first page', () => {
      component.currentPage.set(1);
      fixture.detectChanges();
      const prevButton = nativeElement.querySelector('.pagination-button:first-of-type') as HTMLButtonElement;
      expect(prevButton.disabled).toBeTrue();
    });

    it('should disable the "Next" button on the last page', () => {
      component.currentPage.set(3);
      fixture.detectChanges();
      const nextButton = nativeElement.querySelector('.pagination-button:last-of-type') as HTMLButtonElement;
      expect(nextButton.disabled).toBeTrue();
    });

    it('should disable the "Previous"/"Next" button as pages change', () => {
      component.currentPage.set(1);
      fixture.detectChanges();
      var previousButton = nativeElement.querySelector('.pagination-button:first-of-type') as HTMLButtonElement;
      expect(previousButton.disabled).toBeTrue();

      var nextButton = nativeElement.querySelector('.pagination-button:last-of-type') as HTMLButtonElement;
      expect(nextButton.disabled).toBeFalse();

      component.goToNextPage();
      fixture.detectChanges();

      // We are on page 2 now (or should be) so both should be enabled
      previousButton = nativeElement.querySelector('.pagination-button:first-of-type') as HTMLButtonElement;
      expect(previousButton.disabled).toBeFalse();

      nextButton = nativeElement.querySelector('.pagination-button:last-of-type') as HTMLButtonElement;
      expect(nextButton.disabled).toBeFalse();

      component.goToNextPage();
      fixture.detectChanges();

      // We are on page 3 now (or should be) so next should be disabled
      previousButton = nativeElement.querySelector('.pagination-button:first-of-type') as HTMLButtonElement;
      expect(previousButton.disabled).toBeFalse();

      nextButton = nativeElement.querySelector('.pagination-button:last-of-type') as HTMLButtonElement;
      expect(nextButton.disabled).toBeTrue();

      component.goToPreviousPage();
      fixture.detectChanges();

      // We are on page 2 now (or should be) so both should be disabled
      previousButton = nativeElement.querySelector('.pagination-button:first-of-type') as HTMLButtonElement;
      expect(previousButton.disabled).toBeFalse();

      nextButton = nativeElement.querySelector('.pagination-button:last-of-type') as HTMLButtonElement;
      expect(nextButton.disabled).toBeFalse();
    });
  });

  describe('Helper Methods', () => {
    it('getHostname should extract hostname from a URL', () => {
      expect(component.getHostname('https://www.google.com/search?q=test')).toBe('google.com');
      expect(component.getHostname('http://angular.dev')).toBe('angular.dev');
      expect(component.getHostname('invalid-url')).toBe('');
    });
  });
});
