import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FormsManagement } from './forms-management';

describe('FormsManagement', () => {
  let component: FormsManagement;
  let fixture: ComponentFixture<FormsManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormsManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FormsManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
