import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DataUpdate } from './data-update';

describe('DataUpdate', () => {
  let component: DataUpdate;
  let fixture: ComponentFixture<DataUpdate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataUpdate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DataUpdate);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
