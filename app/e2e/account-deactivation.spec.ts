import { test, expect } from '@playwright/test';

test('Account Deactivation Flow', async ({ page }) => {
    // Capture console logs
    page.on('console', msg => console.log(`BROWSER LOG: ${msg.text()}`));
    page.on('pageerror', err => console.log(`BROWSER ERROR: ${err}`));
    page.on('requestfailed', request => {
        console.log(`REQUEST FAILED: ${request.url()} - ${request.failure()?.errorText}`);
    });

    // 1. Navigation
    console.log('Navigating to root...');
    await page.goto('http://localhost:8081');

    await page.waitForLoadState('networkidle');

    // Check Onboarding
    const onboardingTitle = page.locator('text=Euskal IA').first();
    if (await onboardingTitle.isVisible()) {
        console.log('On Interaction: Clicking Login button...');
        await page.click('text=Ya tengo cuenta');
        await page.waitForTimeout(2000);
    }

    // Login Form
    const inputs = page.locator('input');

    const usernameInput = inputs.first();
    const passwordInput = inputs.nth(1);

    console.log('Filling username...');
    await usernameInput.fill('testuser_fix');

    console.log('Filling password...');
    await passwordInput.fill('password123');

    console.log('Clicking Login...');

    const loginButton = page.locator('text=Iniciar').first();
    if (await loginButton.count() > 0) {
        await loginButton.click();
    } else {
        const buttons = page.locator('div[role="button"]');
        await buttons.last().click();
    }

    // Wait for Home
    console.log('Waiting for Home screen...');
    // Check for stats icons which are likely unique to Home
    // Or "Unidad 1" / "Unit 1"
    const homeIndicator = page.locator('text=🔥').or(page.locator('text=Unidad 1')).or(page.locator('text=Unit 1')).first();

    try {
        await expect(homeIndicator).toBeVisible({ timeout: 10000 });
        console.log('Home screen detected!');
    } catch (e) {
        console.log('Home screen NOT detected.');
        // content dump was too large, rely on console logs from browser
        throw e;
    }

    // 2. Go to Profile
    console.log('Navigating to Profile...');
    const perfilText = page.locator('text=Perfil');
    if (await perfilText.isVisible()) {
        await perfilText.click();
    } else {
        console.log('Profile text not found, clicking Profile Icon (guessing)...');
        const buttonsWithSvg = page.locator('div[role="button"]').filter({ has: page.locator('svg') });
        const count = await buttonsWithSvg.count();
        if (count > 0) {
            const profileButton = buttonsWithSvg.filter({ hasNotText: 'Español' }).first();
            await profileButton.click();
        } else {
            await page.click('xpath=//div[@role="button"][last()]');
        }
    }

    // 3. Click Delete Account
    console.log('Clicking Delete Account...');

    let dialogCount = 0;
    page.on('dialog', async dialog => {
        console.log(`Dialog message: ${dialog.message()}`);
        dialogCount++;
        await dialog.accept();
    });

    // "Eliminar cuenta" or similar
    await page.click('text=Eliminar');

    // Wait for backend response
    const response = await page.waitForResponse(response =>
        response.url().includes('/api/users/request-delete') && response.status() === 200
    );
    expect(response.ok()).toBeTruthy();

    expect(dialogCount).toBeGreaterThanOrEqual(1);
});
