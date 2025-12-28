import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DirectPayment } from './direct-payment';

describe('DirectPayment', () => {
  let component: DirectPayment;
  let fixture: ComponentFixture<DirectPayment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectPayment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DirectPayment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
