import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChildrenData } from './children-data';

describe('ChildrenData', () => {
  let component: ChildrenData;
  let fixture: ComponentFixture<ChildrenData>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChildrenData]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChildrenData);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
