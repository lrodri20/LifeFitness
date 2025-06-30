import React, { useContext } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import {
  TouchableOpacity,
} from 'react-native';
import { AuthProvider, AuthContext } from './context/AuthContext';
import SignInScreen from './screens/SignInScreen';
import SignUpScreen from './screens/SignUpScreen';
import ForgotPasswordScreen from './screens/ForgotPasswordScreen';
import HomeScreen from './screens/HomeScreen';
import SettingsScreen from './screens/SettingsScreen';
import { Ionicons } from '@expo/vector-icons';
const AuthStack = createStackNavigator();
const AppStack = createStackNavigator();

function RootNavigator() {
  const { userToken, isLoading } = useContext(AuthContext);

  // While we restore the token, you could show a splash screen
  if (isLoading) {
    return null;
  }

  return (
    <NavigationContainer>
      {userToken == null ? (
        <AuthStack.Navigator screenOptions={{ headerShown: false }}>
          <AuthStack.Screen name="SignIn" component={SignInScreen} />
          <AuthStack.Screen name="SignUp" component={SignUpScreen} />
          <AuthStack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
        </AuthStack.Navigator>
      ) : (
        <AppStack.Navigator>
          <AppStack.Screen
            name="Home"
            component={HomeScreen}
            options={({ navigation }) => ({
              title: 'Home',
              headerRight: () => (
                <TouchableOpacity
                  onPress={() => navigation.navigate('Settings')}
                  style={{ marginRight: 16 }}
                >
                  <Ionicons name="settings-outline" size={24} color="#000" />
                </TouchableOpacity>
              ),
            })}
          />
          <AppStack.Screen
            name="Settings"
            component={SettingsScreen}
            options={{ title: 'Profile Settings' }}
          />
        </AppStack.Navigator>
      )}
    </NavigationContainer>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <RootNavigator />
    </AuthProvider>
  );
}
