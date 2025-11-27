import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaseFiltersComponent } from './base-filters.component';

describe('BaseFiltersComponent', () => {
  let component: BaseFiltersComponent;
  let fixture: ComponentFixture<BaseFiltersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BaseFiltersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BaseFiltersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
